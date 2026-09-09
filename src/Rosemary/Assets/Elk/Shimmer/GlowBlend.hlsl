#include "../../common.h"

sampler2D Source : register(s0);
sampler2D Destination : register(s1);

TEXTURE_SIZE(TextureSize, 1)

float4 GlowBlendShaderFragment(float2 uv : TEXCOORD0, float2 svPos : SV_POSITION0, float4 baseColor : COLOR0) : COLOR0
{
    float4 color = tex2D(Source, uv) * baseColor;
    float4 base = tex2D(Destination, svPos / TextureSize);
    
    float3 src = color.rgb;
    float3 dst = base.rgb;
    
    float4 result = float4(pow(src, 2) / (1 - dst), 1);

    return result * (1 - pow(1 - color.a, 5)) * base.a;
}

BEGIN_TECHNIQUE(Technique1)
    BEGIN_PASS(GlowBlendShader)       
        PIXEL_SHADER(compile ps_3_0 GlowBlendShaderFragment())        
    END_PASS
END_TECHNIQUE
