#version 330 core
out vec4 FragColor;
in vec3 WorldPos;

uniform samplerCube environmentMap;
uniform float roughness;

const float PI=3.14159265359;
// ----------------------------------------------------------------------------
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
// efficient VanDerCorpus calculation.
float RadicalInverse_VdC(uint bits)
{
    bits=(bits<<16u)|(bits>>16u);
    bits=((bits&0x55555555u)<<1u)|((bits&0xAAAAAAAAu)>>1u);
    bits=((bits&0x33333333u)<<2u)|((bits&0xCCCCCCCCu)>>2u);
    bits=((bits&0x0F0F0F0Fu)<<4u)|((bits&0xF0F0F0F0u)>>4u);
    bits=((bits&0x00FF00FFu)<<8u)|((bits&0xFF00FF00u)>>8u);
    return float(bits)*2.3283064365386963e-10;// / 0x100000000
}
// ----------------------------------------------------------------------------
vec2 Hammersley(uint i,uint N)
{
    return vec2(float(i)/float(N),RadicalInverse_VdC(i));
}
// ----------------------------------------------------------------------------
vec3 ImportanceSampleGGX(vec2 Xi,vec3 N,float roughness)
{
    float a=roughness*roughness;
    
    float phi=2.*PI*Xi.x;
    float cosTheta=sqrt((1.-Xi.y)/(1.+(a*a-1.)*Xi.y));
    float sinTheta=sqrt(1.-cosTheta*cosTheta);
    
    // from spherical coordinates to cartesian coordinates - halfway vector
    vec3 H;
    H.x=cos(phi)*sinTheta;
    H.y=sin(phi)*sinTheta;
    H.z=cosTheta;
    
    // from tangent-space H vector to world-space sample vector
    vec3 up=abs(N.z)<.999?vec3(0.,0.,1.):vec3(1.,0.,0.);
    vec3 tangent=normalize(cross(up,N));
    vec3 bitangent=cross(N,tangent);
    
    vec3 sampleVec=tangent*H.x+bitangent*H.y+N*H.z;
    return normalize(sampleVec);
}

void main()
{
    vec3 N=normalize(WorldPos);
    // make the simplifying assumption that V equals R equals the normal
    vec3 R=N;
    vec3 V=R;
    
    const uint SAMPLE_COUNT=1024u;
    vec3 prefilteredColor=vec3(0.);
    float totalWeight=0.;
    
    for(uint i=0u;i<SAMPLE_COUNT;++i)
    {
        // generates a sample vector that's biased towards the preferred alignment direction (importance sampling).
        vec2 Xi=Hammersley(i,SAMPLE_COUNT);
        vec3 H=ImportanceSampleGGX(Xi,N,roughness);
        vec3 L=normalize(2.*dot(V,H)*H-V);
        
        float NdotL=max(dot(N,L),0.);
        if(NdotL>0.)
        {
            // sample from the environment's mip level based on roughness/pdf
            float D=DistributionGGX(N,H,roughness);
            float NdotH=max(dot(N,H),0.);
            float HdotV=max(dot(H,V),0.);
            float pdf=D*NdotH/(4.*HdotV)+.0001;
            //这里计算样本的概率密度函数（PDF）。PDF 用于重要性采样，它告诉我们当前采样方向的重要性。0.0001 是一个小常数，用来避免除以零
            float resolution=512.;// resolution of source cubemap (per face)
            float saTexel=4.*PI/(6.*resolution*resolution);
            //计算每个纹素（贴图像素）在立方体贴图上覆盖的立体角（Solid Angle）。
            //这是通过将整个球面面积 4 * PI 除以贴图的总纹素数（6 * resolution * resolution，因为立方体贴图有6个面）来计算得到的。
            float saSample=1./(float(SAMPLE_COUNT)*pdf+.0001);
            //计算每个采样在球面上所占的平均立体角。SAMPLE_COUNT 是采样数，pdf 是之前计算的每个样本的概率密度函数。
            //这里同样加上了一个小常数以避免除以零。
            float mipLevel=roughness==0.?0.:.5*log2(saSample/saTexel);
            //根据粗糙度和采样的立体角与纹素的立体角的比率，计算出应当从环境贴图的哪一级MIP贴图中取样。
            //如果粗糙度为0，表明表面是完美镜面，所以直接取MIP级别0（最不模糊的级别）。
            //对于非零粗糙度，根据立体角的比率，使用 log2 函数来决定MIP级别，
            //在纹理映射的MIP映射中，每上升一个MIP级别，纹理的分辨率就在每个维度上降低一半，总体大小降低到原来的四分之一。
            //因此，纹理的分辨率与MIP级别之间的关系是指数性的。这意味着纹理分辨率是2的MIP级别次幂的倒数。
            //在上面的公式中，saSample / saTexel 给出了采样立体角与单个纹素立体角的比率。
            //因此，log2(saSample / saTexel) 给出了在对数尺度上的立体角比率，这是一个无单位的比值。、
            //由于我们知道MIP级别与分辨率的关系是每上升一级，分辨率降低一半，所以我们需要乘以0.5来将这个比值转换成MIP级别的增量。
            //简而言之，0.5这个系数来自于MIP级别与分辨率之间的对数关系，确保了当纹理分辨率每减半时，MIP级别增加1。
            prefilteredColor+=textureLod(environmentMap,L,mipLevel).rgb*NdotL;
            totalWeight+=NdotL;
        }
    }
    prefilteredColor=prefilteredColor/totalWeight;
    FragColor=vec4(prefilteredColor,1.);
}