#version 330 core
out vec4 FragColor;
in vec3 WorldPos;

uniform samplerCube environmentMap;

const float PI=3.14159265359;

void main()
{
    // 将世界位置向量标准化，得到该点的法线向量
    vec3 N=normalize(WorldPos);
    
    // 初始化辐照度向量为零
    vec3 irradiance=vec3(0.);
    
    // 定义上方向，初始化右方向向量
    vec3 up=vec3(0.,1.,0.);
    vec3 right=normalize(cross(up,N));
    // 重新计算上方向，确保它垂直于N和right
    up=normalize(cross(N,right));
    
    // 采样步长和采样数
    float sampleDelta=.025;
    float nrSamples=0.;
    
    // 循环遍历所有采样方向，phi是方位角，theta是极角
    for(float phi=0.;phi<2.*PI;phi+=sampleDelta)
    {
        for(float theta=0.;theta<.5*PI;theta+=sampleDelta)
        {
            // 将球坐标转换为笛卡尔坐标（在切线空间中）
            vec3 tangentSample=vec3(sin(theta)*cos(phi),sin(theta)*sin(phi),cos(theta));
            // 将切线空间的向量转换为世界空间
            vec3 sampleVec=tangentSample.x*right+tangentSample.y*up+tangentSample.z*N;
            
            // 从环境贴图中取样，累加辐照度
            // 使用 Lambert's Cosine Law 加权每个样本的贡献
            irradiance+=texture(environmentMap,sampleVec).rgb*cos(theta)*sin(theta);
            nrSamples++;
        }
    }
    
    // 将累加的辐照度除以样本数量并乘以PI，得到平均辐照度
    irradiance=PI*irradiance*(1./float(nrSamples));
    
    // 输出辐照度，alpha通道设置为1
    FragColor=vec4(irradiance,1.);
}