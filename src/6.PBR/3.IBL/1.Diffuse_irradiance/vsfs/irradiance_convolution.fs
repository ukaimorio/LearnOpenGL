#version 330 core
out vec4 FragColor;
in vec3 WorldPos;

uniform samplerCube environmentMap;

const float PI=3.14159265359;

void main()
{
    // 世界向量充当切线面上的法线
    // 从原点出发，与 WorldPos 对齐。给定该法线，计算环境的所有入射辐射。
    // 此辐射的结果是来自 -Normal 方向的光辐射，这是我们在 PBR 着色器中用于采样入射辐射的内容。
    vec3 N=normalize(WorldPos);
    
    vec3 irradiance=vec3(0.);
    
    // 从原点点出发的切线空间计算
    vec3 up=vec3(0.,1.,0.);
    vec3 right=normalize(cross(up,N));
    up=normalize(cross(N,right));
    
    float sampleDelta=.025;
    float nrSamples=0.;
    for(float phi=0.;phi<2.*PI;phi+=sampleDelta)
    {
        for(float theta=0.;theta<.5*PI;theta+=sampleDelta)
        {
            // 球坐标转换为笛卡尔坐标（在切线空间中）
            vec3 tangentSample=vec3(sin(theta)*cos(phi),sin(theta)*sin(phi),cos(theta));
            // 切线空间转换为世界空间
            vec3 sampleVec=tangentSample.x*right+tangentSample.y*up+tangentSample.z*N;
            
            irradiance+=texture(environmentMap,sampleVec).rgb*cos(theta)*sin(theta);
            nrSamples++;
        }
    }
    irradiance=PI*irradiance*(1./float(nrSamples));
    
    FragColor=vec4(irradiance,1.);
}
