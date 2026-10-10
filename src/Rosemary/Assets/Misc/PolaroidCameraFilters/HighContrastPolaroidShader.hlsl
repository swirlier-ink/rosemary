#include "../../common.h"

sampler2D Texture : register(s0);

float Contrast;

float4 HighContrastPolaroidShaderFragment(float4 baseColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float4 c = tex2D(Texture, uv);
    
    c.rgb = ((c.rgb - 0.5) * max(Contrast, 0)) + 0.5;

    return c * baseColor;
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(HighContrastPolaroidShader)  
        PIXEL_SHADER(compile ps_3_0 HighContrastPolaroidShaderFragment())   
    END_PASS
END_TECHNIQUE