#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoords;

struct Material
{
    sampler2D diffuse;
    sampler2D specular;
    
    float shininess;
};

struct Light
{
    //vec3 direction;
    vec3 position;
    
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    
    float constant;
    float linear;
    float quadratic;
};

uniform vec3 viewPos;
uniform Material material;
uniform Light light;

void main()
{
    
    float distance=length(light.position-FragPos);
    float attenuation=1./(light.constant+light.linear*distance+light.quadratic*(distance*distance));
    
    vec3 ambient=light.ambient*vec3(texture(material.diffuse,TexCoords).rgb);
    
    vec3 norm=normalize(Normal);
    vec3 lightDir=normalize(light.position-FragPos);
    float diff=max(dot(norm,lightDir),0.);
    vec3 diffuse=diff*light.diffuse*vec3(texture(material.diffuse,TexCoords).rgb);
    
    vec3 viewDir=normalize(viewPos-FragPos);
    vec3 reflectDir=reflect(-lightDir,norm);
    float spec=pow(max(dot(viewDir,reflectDir),0.),16.);
    vec3 specular=spec*light.specular*vec3(texture(material.specular,TexCoords).rgb);
    
    vec3 result=ambient+diffuse+specular;
    result*=attenuation*1.5;
    FragColor=vec4(result,1.);
}