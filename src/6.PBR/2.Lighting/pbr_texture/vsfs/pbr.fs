#version 330 core
out vec4 FragColor;
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

// 材质参数
uniform sampler2D albedoMap;// 反照率贴图
uniform sampler2D normalMap;// 法线贴图
uniform sampler2D metallicMap;// 金属度贴图
uniform sampler2D roughnessMap;// 粗糙度贴图
uniform sampler2D aoMap;// 环境光遮蔽贴图

// 光源
uniform vec3 lightPositions[4];// 光源位置数组
uniform vec3 lightColors[4];// 光源颜色数组

uniform vec3 camPos;// 相机位置

const float PI=3.14159265359;
// ----------------------------------------------------------------------------
// 一种获取切线法线到世界坐标的简便技巧，以简化PBR代码。
// 不用担心是否理解，通常出于性能考虑，您仍然希望以常规方式进行法线映射。
vec3 getNormalFromMap()
{
    vec3 tangentNormal=texture(normalMap,TexCoords).xyz*2.-1.;
    
    vec3 Q1=dFdx(WorldPos);
    vec3 Q2=dFdy(WorldPos);
    vec2 st1=dFdx(TexCoords);
    vec2 st2=dFdy(TexCoords);
    
    vec3 N=normalize(Normal);
    vec3 T=normalize(Q1*st2.t-Q2*st1.t);
    vec3 B=-normalize(cross(N,T));
    mat3 TBN=mat3(T,B,N);
    
    return normalize(TBN*tangentNormal);
}
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
// GGX几何遮挡函数的Schlick近似
float GeometrySchlickGGX(float NdotV,float roughness)
{
    float r=(roughness+1.);
    float k=(r*r)/8.;
    
    float nom=NdotV;
    float denom=NdotV*(1.-k)+k;
    
    return nom/denom;
}
// ----------------------------------------------------------------------------
// GGX几何遮挡函数
float GeometrySmith(vec3 N,vec3 V,vec3 L,float roughness)
{
    float NdotV=max(dot(N,V),0.);
    float NdotL=max(dot(N,L),0.);
    float ggx2=GeometrySchlickGGX(NdotV,roughness);
    float ggx1=GeometrySchlickGGX(NdotL,roughness);
    
    return ggx1*ggx2;
}
// ----------------------------------------------------------------------------
// Schlick近似的菲涅尔反射
vec3 fresnelSchlick(float cosTheta,vec3 F0)
{
    return F0+(1.-F0)*pow(clamp(1.-cosTheta,0.,1.),5.);
}
// ----------------------------------------------------------------------------
void main()
{
    vec3 albedo=pow(texture(albedoMap,TexCoords).rgb,vec3(2.2));
    float metallic=texture(metallicMap,TexCoords).r;
    float roughness=texture(roughnessMap,TexCoords).r;
    float ao=texture(aoMap,TexCoords).r;
    
    vec3 N=getNormalFromMap();
    vec3 V=normalize(camPos-WorldPos);
    
    // 在法线入射时计算反射率；如果是电介质（如塑料），使用F0为0.04，
    // 如果是金属，使用反照率颜色作为F0（金属工作流）
    vec3 F0=vec3(.04);
    F0=mix(F0,albedo,metallic);
    
    // 反射率方程
    vec3 Lo=vec3(0.);
    for(int i=0;i<4;++i)
    {
        // 计算每个光源的辐射
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
        float denominator=4.*max(dot(N,V),0.)*max(dot(N,L),0.)+.0001;// + 0.0001以防止除以零
        vec3 specular=numerator/denominator;
        
        // kS等于菲涅尔反射
        vec3 kS=F;
        // 为了能量守恒，漫反射和镜面反射光不能超过1.0（除非表面发光）；
        // 为了保持这种关系，漫反射分量（kD）应等于1.0 - kS。
        vec3 kD=vec3(1.)-kS;
        // 将kD乘以金属度的逆，以使只有非金属物体有漫反射光，或者在部分金属的情况下进行线性混合
        kD*=1.-metallic;
        
        // 缩放光线通过NdotL
        float NdotL=max(dot(N,L),0.);
        
        // 添加到出射辐射Lo
        Lo+=(kD*albedo/PI+specular)*radiance*NdotL;// 注意我们已经将BRDF乘以菲涅尔（kS），因此不再乘以kS
    }
    
    // 环境光照（注意，下一个IBL教程将用环境光照替代这个环境光照）。
    vec3 ambient=vec3(.03)*albedo*ao;
    
    vec3 color=ambient+Lo;
    
    // HDR色调映射
    color=color/(color+vec3(1.));
    // Gamma校正
    color=pow(color,vec3(1./2.2));
    
    FragColor=vec4(color,1.);
}
