#include "../../common.h"

sampler2D Texture : register(s0);

float Power;
float Preservation;

float4 MonochromePolaroidShaderFragment(float4 baseColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float4 c = tex2D(Texture, uv);
    
    float value = pow((c.r + c.g + c.b) / 3.0, Power);

    return lerp(float4(baseColor.rgb * value, c.a), c, Preservation);
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(MonochromePolaroidShader)  
        PIXEL_SHADER(compile ps_3_0 MonochromePolaroidShaderFragment())   
    END_PASS
END_TECHNIQUE