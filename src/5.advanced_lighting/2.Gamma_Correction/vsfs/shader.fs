#version 330 core
out vec4 FragColor;

in VS_OUT{
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
}fs_in;

uniform sampler2D floorTexture;

uniform vec3 lighhtPositions[4];
uniform vec3 lightColors[4];
uniform vec3 viewPos;
uniform bool gamma;

vec3 BlinnPhong(vec3 normal,vec3 FragPos,vec3 lightPos,vec3 lightColor)
{
    vec3 lightDir=normalize(lightPos-FragPos);
    float diff=max(dot(lightDir,normal),0.);
    vec3 diffuse=diff*lightColor;
    
    vec3 viewDir=normalize(viewPos-FragPos);
    vec3 reflectDir=reflect(-lightDir,normal);
    float spec=0.;
    vec3 halfwayDir=normalize(lightDir+viewDir);
    spec=pow(max(dot(normal,halfwayDir),0.),64.);
    vec3 specular=spec*lightColor;
    
    float max_distance=1.5;
    float distance=length(lightPos-FragPos);
    float attenuation=1./(gamma?distance*distance:distance);
    
    diffuse*=attenuation;
    specular*=attenuation;
    
    return diffuse+specular;
}

void main()
{
    vec3 color=texture(floorTexture,fs_in.TexCoords).rgb;
    vec3 lighting=vec3(0.);
    for(int i=0;i<4;i++)
    {
        lighting+=BlinnPhong(fs_in.Normal,fs_in.FragPos,lighhtPositions[i],lightColors[i]);
    }
    color*=lighting;
    if(gamma)
    {
        color=pow(color,vec3(1./2.2));
    }
    FragColor=vec4(color,1.);
}