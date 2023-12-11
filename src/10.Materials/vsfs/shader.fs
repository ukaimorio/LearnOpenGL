#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;

struct Material
{
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    
    float shininess;
};

struct Light
{
    vec3 position;
    
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

uniform vec3 viewPos;
uniform Material material;
uniform Light light;

void main()
{
    vec3 ambient=light.ambient*material.ambient;
    
    vec3 lightColor=light.diffuse;
    vec3 objectColor=material.diffuse;
    vec3 lightDir=normalize(light.position-FragPos);
    float diff=max(dot(Normal,lightDir),0.);
    vec3 diffuse=diff*lightColor*objectColor;
    
    vec3 viewDir=normalize(viewPos-FragPos);
    vec3 reflectDir=reflect(-lightDir,Normal);
    float spec=pow(max(dot(viewDir,reflectDir),0.),material.shininess);
    vec3 specular=spec*light.specular*material.specular;
    
    vec3 result=ambient+diffuse+specular;
    FragColor=vec4(result,1.);
}