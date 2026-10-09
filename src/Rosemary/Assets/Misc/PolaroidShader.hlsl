#include "../common.h"

sampler2D Texture : register(s0);
sampler2D Noise : register(s1);

#define BAYER              (float4x4(0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5) / 16)
#define CONTRAST           (1.1)
#define NOISE_SCALE        (5)
#define NOISE_INTENSITY    (0.05)

float Random;
float2 Size;

float4 PolaroidShaderFragment(float4 baseColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    float2 bayeruv = frac(Size * uv / 4) * 4;
    
    float4 c = tex2D(Texture, uv);
    c = floor(c * 16) / 16;
    c = pow(c, CONTRAST);
    
    float4 n = tex2D(Noise, uv * NOISE_SCALE + float2(Random, Random));
    
    float4 final = c * (1.0 - NOISE_INTENSITY) + n * NOISE_INTENSITY - (BAYER[bayeruv.x][bayeruv.y]) * NOISE_INTENSITY;
    
    return final * baseColor;
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(PolaroidShader) 
        PIXEL_SHADER(compile ps_3_0 PolaroidShaderFragment())  
    END_PASS
END_TECHNIQUE