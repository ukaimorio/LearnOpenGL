#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;

uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 objectColor;
uniform vec3 viewPos;

void main()
{
    float ammbientStrength=.1;
    vec3 ambient=ammbientStrength*lightColor;
    
    vec3 norm=normalize(Normal);
    vec3 lightDir=normalize(lightPos-FragPos);
    float diff=max(dot(norm,lightDir),0.);
    vec3 diffuse=diff*lightColor;
    
    float specularStrength=.5;
    vec3 viewDir=normalize(viewPos-FragPos);
    vec3 reflectDir=reflect(-lightDir,norm);
    float spec=pow(max(dot(viewDir,reflectDir),0.),32);
    vec3 specular=specularStrength*spec*lightColor;
    
    vec3 result=(ambient+diffuse+specular)*objectColor;
    FragColor=vec4(result,1.);
}