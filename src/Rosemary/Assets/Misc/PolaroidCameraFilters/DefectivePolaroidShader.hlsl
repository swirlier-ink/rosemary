#include "../../common.h"
#include "../../colors.h"


float4 AmbientLight;
float4 TintPenis;
float TemperatureApplyAmount;

float2 UnderexposurePos;
float2 UnderexposurePow;


sampler2D Texture : register(s0);

float4 DefectivePolaroidShaderFragment(float4 baseColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float4 amb = float4(1, 1, 1, 1) - pow(AmbientLight + float4(0.5, 0.5, 0.5, 0.5), 2) - float4(0.5, 0.5, 0.5, 0.5);

    float4 c = tex2D(Texture, uv);
    float4 color = c;

    // minor precompensation
    float penus = color.r * 0.23;
    color.r -= penus;
    color.g += penus;
    
    

    // here im employing what i call "channel compensation"
    // for each channel, we take the AmbientLight value (the amount of compenstatoin)
    // and then we reduce the two other channels by that amount
    // and take what we reduced and add it to the original channel
    float2 rComp = c.gb * amb.r;
    float2 gComp = c.rb * amb.g;
    float2 bComp = c.rg * amb.b;

    color.gb -= rComp;
    color.r += (rComp.x + rComp.y) * 0.08;
    color.rg -= bComp;
    color.b += (bComp.x + bComp.y) * 0.2;
    color.rb -= gComp;
    color.g += (gComp.x + gComp.y) * 0.2;

    
    // then we do "yellow compensation"
    float yellowComp = 0.2 * ((amb.r + amb.g) * 1.0 - amb.b);
    color.rg *= float2(1.0 + yellowComp, 1.0 + yellowComp);

    // then we ad some nonexposure
    float white = saturate(
                            (uv.x - UnderexposurePos.x) * UnderexposurePow.x +
                            (uv.y - UnderexposurePos.y) * UnderexposurePow.y
                            );

    
    
    float3 chsl = RGBToHSL(color.rgb);
    chsl.y *= 0.8 - (amb.b * 0.1);
    float4 postAdj = float4(HSLToRGB(chsl), 1);

    postAdj = lerp(postAdj, TintPenis, TemperatureApplyAmount);
    // contrast compensation
    postAdj.rgb = ((postAdj.rgb - 0.5) * max(1 + TemperatureApplyAmount, 0)) + 0.5;
    
    postAdj += white;
    postAdj = lerp(postAdj, AmbientLight, 0.04);
    return postAdj * baseColor;
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(DefectivePolaroidShader)  
        PIXEL_SHADER(compile ps_3_0 DefectivePolaroidShaderFragment())   
    END_PASS
END_TECHNIQUE