#include "../common.h"

sampler2D Texture : register(s0);

float Power = 5;
float Intensity = 4;

float4 VignetteShaderFragment(float4 baseColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    float4 c = tex2D(Texture, uv); 
    
    float2 vignette = (uv.xy - float2(0.5, 0.5));
    
    c -= (pow(abs(vignette.x * 2), 44) + pow(abs(vignette.y * 2), 44)) * 0.3;
    
    c -= pow(length(vignette), Power) * Intensity;
    
    return c * baseColor;
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(VignetteShader) 
        PIXEL_SHADER(compile ps_3_0 VignetteShaderFragment())  
    END_PASS
END_TECHNIQUE