#version 330 core
out vec4 FragColor;
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

// 材质参数
uniform vec3 albedo;// 反照率
uniform float metallic;// 金属度
uniform float roughness;// 粗糙度
uniform float ao;// 环境遮挡

// IBL（Image-Based Lighting）辐照度立方体贴图
uniform samplerCube irradianceMap;

// 光源
uniform vec3 lightPositions[4];// 光源位置数组
uniform vec3 lightColors[4];// 光源颜色数组

uniform vec3 camPos;// 相机位置

const float PI=3.14159265359;

// ----------------------------------------------------------------------------
// GGX分布函数
float DistributionGGX(vec3 N,vec3 H,float roughness)
{
    float a=roughness*roughness;
    float a2=a*a;
    float NdotH=max(dot(N,H),0.);
    float NdotH2=NdotH*NdotH;
    
    float nom=a2;
    float denom=(NdotH2*(a2-1.)+1.);
    denom=PI*denom*denom;
    
    return nom/denom;
}

// ----------------------------------------------------------------------------
// Schlick-GGX几何函数
float GeometrySchlickGGX(float NdotV,float roughness)
{
    float r=(roughness+1.);
    float k=(r*r)/8.;
    
    float nom=NdotV;
    float denom=NdotV*(1.-k)+k;
    
    return nom/denom;
}

// ----------------------------------------------------------------------------
// Smith几何遮挡函数
float GeometrySmith(vec3 N,vec3 V,vec3 L,float roughness)
{
    float NdotV=max(dot(N,V),0.);
    float NdotL=max(dot(N,L),0.);
    float ggx2=GeometrySchlickGGX(NdotV,roughness);
    float ggx1=GeometrySchlickGGX(NdotL,roughness);
    
    return ggx1*ggx2;
}

// ----------------------------------------------------------------------------
// Schlick近似的菲涅耳反射
vec3 fresnelSchlick(float cosTheta,vec3 F0)
{
    return F0+(1.-F0)*pow(clamp(1.-cosTheta,0.,1.),5.);
}

vec3 fresnelSchlickRoughness(float cosTheta,vec3 F0,float roughness)
{
    return F0+(max(vec3(1.-roughness),F0)-F0)*pow(1.-cosTheta,5.);
}
// ----------------------------------------------------------------------------
void main()
{
    vec3 N=Normal;
    vec3 V=normalize(camPos-WorldPos);
    vec3 R=reflect(-V,N);
    
    // 在法线方向上计算入射反射率；如果是非金属（如塑料），使用F0为0.04，
    // 如果是金属，则使用反照率颜色作为F0（金属工作流）
    vec3 F0=vec3(.04);
    F0=mix(F0,albedo,metallic);
    
    // 反射率方程
    vec3 Lo=vec3(0.);
    for(int i=0;i<4;++i)
    {
        // 计算每个光源的辐亮度
        vec3 L=normalize(lightPositions[i]-WorldPos);
        vec3 H=normalize(V+L);
        float distance=length(lightPositions[i]-WorldPos);
        float attenuation=1./(distance*distance);
        vec3 radiance=lightColors[i]*attenuation;
        
        // Cook-Torrance BRDF
        float NDF=DistributionGGX(N,H,roughness);
        float G=GeometrySmith(N,V,L,roughness);
        vec3 F=fresnelSchlick(max(dot(H,V),0.),F0);
        
        vec3 numerator=NDF*G*F;
        float denominator=4.*max(dot(N,V),0.)*max(dot(N,L),0.)+.0001;// + 0.0001 防止除零
        vec3 specular=numerator/denominator;
        
        // kS 等于 Fresnel
        vec3 kS=F;
        // 为了能量守恒，漫反射和镜面反射的光照不能超过1.0（除非表面发光）；
        // 为了保持这种关系，漫反射分量（kD）应等于 1.0 - kS。
        vec3 kD=vec3(1.)-kS;
        // 将 kD 乘以反金属度的倒数，以便只有非金属材料有漫反射光照，
        // 或者在部分金属时进行线性混合（纯金属没有漫反射光照）。
        kD*=1.-metallic;
        
        // 将光照按 NdotL 缩放
        float NdotL=max(dot(N,L),0.);
        
        // 加到出射辐亮度 Lo 上
        Lo+=(kD*albedo/PI+specular)*radiance*NdotL;// 注意我们已经将BRDF乘以了Fresnel（kS），因此我们不会再次乘以kS
        
    }
    
    // 环境光照（现在我们使用IBL作为环境项）
    vec3 kS=fresnelSchlick(max(dot(N,V),0.),F0);
    //vec3 kS=fresnelSchlickRoughness(max(dot(N,V),0.),F0,roughness);
    vec3 kD=1.-kS;
    kD*=1.-metallic;
    vec3 irradiance=texture(irradianceMap,N).rgb;
    vec3 diffuse=irradiance*albedo;
    vec3 ambient=(kD*diffuse)*ao;
    // vec3 ambient = vec3(0.002);
    
    vec3 color=ambient+Lo;
    
    // HDR 色调映射
    color=color/(color+vec3(1.));
    // 伽马校正
    color=pow(color,vec3(1./2.2));
    
    FragColor=vec4(color,1.);
}
