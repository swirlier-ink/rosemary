#include "../../../common.h"

sampler2D Texture : register(s0);

#define SAMPLE_COUNT (16)
#define SAMPLE_COUNT_HALF (8)

static const float kernel[8] = { 0.1974, 0.1747, 0.1210, 0.0656, 0.0278, 0.0092, 0.0024, 0.0005 };

float2 BlurSize;

float4 HorizontalShaderFragment(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = 0;
    
    float2 dtc = BlurSize / SAMPLE_COUNT;
    dtc.y = 0;
    
    [unroll(SAMPLE_COUNT_HALF)]
    for (int i = 1; i < SAMPLE_COUNT_HALF; i++)
    {
        float weight = kernel[i];
        color += tex2D(Texture, uv + dtc * i) * weight;
        color += tex2D(Texture, uv - dtc * i) * weight;
    }
    color += tex2D(Texture, uv) * kernel[0];
    
    return color;
}

float4 VerticalShaderFragment(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = 0;
    
    float2 dtc = BlurSize / SAMPLE_COUNT;
    dtc.x = 0;
    
    [unroll(SAMPLE_COUNT_HALF)]
    for (int i = 1; i < SAMPLE_COUNT_HALF; i++)
    {
        float weight = kernel[i];
        color += tex2D(Texture, uv + dtc * i) * weight;
        color += tex2D(Texture, uv - dtc * i) * weight;
    }
    color += tex2D(Texture, uv) * kernel[0];
    
    return color;
}

float4 DirectionalShaderFragment(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = 0;
    
    float2 dtc = BlurSize / SAMPLE_COUNT;
    
    [unroll(SAMPLE_COUNT_HALF)]
    for (int i = 1; i < SAMPLE_COUNT_HALF; i++)
    {
        float weight = kernel[i];
        color += tex2D(Texture, uv + dtc * i) * weight;
        color += tex2D(Texture, uv - dtc * i) * weight;
    }
    color += tex2D(Texture, uv) * kernel[0];
    
    return color;
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(HorizontalShader) 
        PIXEL_SHADER(compile ps_3_0 HorizontalShaderFragment()) 
    END_PASS
    BEGIN_PASS(VerticalShader) 
        PIXEL_SHADER(compile ps_3_0 VerticalShaderFragment()) 
    END_PASS
    BEGIN_PASS(DirectionalShader)   
        PIXEL_SHADER(compile ps_3_0 DirectionalShaderFragment())   
    END_PASS
END_TECHNIQUE