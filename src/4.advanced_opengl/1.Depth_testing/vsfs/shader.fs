#version 330 core
out vec4 FragColor;

float near=.1;
float far=100.;

float LinearizeDepth(float depth)
{
    float z=depth*2.-1.;// back to NDC
    return(2.*near*far)/(far+near-z*(far-near));
}

void main()
{
    float depth=LinearizeDepth(gl_FragCoord.z)/far;// 为了演示除以 far
    FragColor=vec4(vec3(depth),1.);
}