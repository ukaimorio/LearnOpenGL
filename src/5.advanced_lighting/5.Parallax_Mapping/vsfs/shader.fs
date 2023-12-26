#version 330 core
out vec4 FragColor;

in VS_OUT{
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
}fs_in;

uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
uniform sampler2D depthMap;

uniform float heightScale;

vec2 ParallaxMapping(vec2 texCoords,vec3 viewDir)
{
    // 深度层数量的范围
    const float minLayers=8;
    const float maxLayers=32;
    
    // 根据视点方向与正 z 方向的夹角来计算深度层数量
    float numLayers=mix(maxLayers,minLayers,abs(dot(vec3(0.,0.,1.),viewDir)));
    
    // 计算每个深度层的大小
    float layerDepth=1./numLayers;
    
    // 当前深度层的深度值
    float currentLayerDepth=0.;
    
    // 每个深度层中纹理坐标移动的量（来自向量 P）
    vec2 P=viewDir.xy/viewDir.z*heightScale;
    vec2 deltaTexCoords=P/numLayers;
    
    // 获取初始值
    vec2 currentTexCoords=texCoords;
    float currentDepthMapValue=texture(depthMap,currentTexCoords).r;
    
    // 遍历深度层
    while(currentLayerDepth<currentDepthMapValue)
    {
        // 沿着 P 的方向移动纹理坐标
        currentTexCoords-=deltaTexCoords;
        
        // 获取当前纹理坐标处的深度图值
        currentDepthMapValue=texture(depthMap,currentTexCoords).r;
        
        // 更新下一层的深度值
        currentLayerDepth+=layerDepth;
    }
    
    // 获取碰撞前的纹理坐标（反向操作）
    vec2 prevTexCoords=currentTexCoords+deltaTexCoords;
    
    // 获取碰撞后和碰撞前的深度值，用于线性插值
    float afterDepth=currentDepthMapValue-currentLayerDepth;
    float beforeDepth=texture(depthMap,prevTexCoords).r-currentLayerDepth+layerDepth;
    
    // 纹理坐标的插值
    float weight=afterDepth/(afterDepth-beforeDepth);
    vec2 finalTexCoords=prevTexCoords*weight+currentTexCoords*(1.-weight);
    
    return finalTexCoords;
}

void main()
{
    // offset texture coordinates with Parallax Mapping
    vec3 viewDir=normalize(fs_in.TangentViewPos-fs_in.TangentFragPos);
    vec2 texCoords=fs_in.TexCoords;
    
    texCoords=ParallaxMapping(fs_in.TexCoords,viewDir);
    if(texCoords.x>1.||texCoords.y>1.||texCoords.x<0.||texCoords.y<0.)
    discard;
    
    // obtain normal from normal map
    vec3 normal=texture(normalMap,texCoords).rgb;
    normal=normalize(normal*2.-1.);
    
    // get diffuse color
    vec3 color=texture(diffuseMap,texCoords).rgb;
    // ambient
    vec3 ambient=.1*color;
    // diffuse
    vec3 lightDir=normalize(fs_in.TangentLightPos-fs_in.TangentFragPos);
    float diff=max(dot(lightDir,normal),0.);
    vec3 diffuse=diff*color;
    // specular
    vec3 reflectDir=reflect(-lightDir,normal);
    vec3 halfwayDir=normalize(lightDir+viewDir);
    float spec=pow(max(dot(normal,halfwayDir),0.),32.);
    
    vec3 specular=vec3(.2)*spec;
    FragColor=vec4(ambient+diffuse+specular,1.);
}