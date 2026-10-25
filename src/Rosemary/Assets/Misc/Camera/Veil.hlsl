#include "../../common.h"

sampler2D Texture : register(s0);
sampler2D Noise : register(s1);
sampler2D SwirlierNoises : register(s2);

float4 Bounds;
float Time;

float FadeDistance;

VIEWPORT_SIZE(ScreenSize);

float Map(float value, float start1, float stop1, float start2, float stop2)
{
    value = clamp(value, start1, stop1);
    return start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));
}

float4 VeilShaderFragment(float2 uv : TEXCOORD0, float2 svPos : SV_POSITION) : COLOR0
{
    float2 screenUv = svPos;
    screenUv /= ScreenSize;
    
    float2 offset = tex2D(Noise, screenUv * float2(0.2, 1)).g;
    offset -= 0.5;
    offset.x = 0;
    
    offset /= ScreenSize;
    
    offset *= 3;
    
    const float fluff = 0.01;
    
    float gradient = 0;
    gradient += Map(svPos.x, Bounds.x - FadeDistance, Bounds.x + fluff, 1, 0);
    gradient += Map(svPos.y, Bounds.y - FadeDistance, Bounds.y + fluff, 1, 0);
    
    gradient += Map(svPos.x, Bounds.z - fluff, Bounds.z + FadeDistance, 0, 1);
    gradient += Map(svPos.y, Bounds.w - fluff, Bounds.w + FadeDistance, 0, 1);
    
    gradient = saturate(gradient);
    
    gradient += pow(tex2D(SwirlierNoises, (screenUv * 0.65) + Time), 2) * gradient;
    
    gradient = pow(gradient, 1.6);
    gradient = 1 - gradient;
    
    float4 color = tex2D(Texture, uv + offset);
    color *= gradient;
    
    return color;
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(VeilShader)  
        PIXEL_SHADER(compile ps_3_0 VeilShaderFragment())   
    END_PASS
END_TECHNIQUE