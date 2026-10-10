#include "../../common.h"

sampler2D Texture : register(s0);

float Power;

float4 GreenPolaroidShaderFragment(float4 baseColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float4 c = tex2D(Texture, uv);
    
    float value = pow((c.r + c.g + c.b) / 3.0, Power);

    return float4(0, value, 0, c.a);
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(GreenPolaroidShader)  
        PIXEL_SHADER(compile ps_3_0 GreenPolaroidShaderFragment())   
    END_PASS
END_TECHNIQUE