#version 330 core
out vec4 FragColor;

in vec2 TexCoords;
in vec3 FragPos;
in vec3 Normal;

uniform vec3 cameraPos;
uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;
uniform sampler2D texture_reflection1;
uniform samplerCube texture_skybox;
struct Light
{
    vec3 direction;
    
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};
uniform Light light;

void main()
{
    // 计算漫反射
    vec3 I=normalize(FragPos-cameraPos);
    vec3 normal=normalize(Normal);
    vec3 R=reflect(I,normal);
    vec3 lightDir=normalize(-light.direction);
    float diff=max(dot(normal,lightDir),0.);
    vec3 diffuse=light.diffuse*diff*texture(texture_diffuse1,TexCoords).rgb;
    // 计算反射光
    vec3 envColor=texture(texture_skybox,R).rgb*texture(texture_reflection1,TexCoords).rgb;
    // 合并所有光照
    vec3 result=diffuse+envColor;
    // 输出最终颜色
    FragColor=vec4(result,1);
}