float time;
float blowUpPower;
float blowUpSize;
float3 shieldColor;
float shieldOpacity;
float3 shieldEdgeColor;
float shieldEdgeBlendStrenght;
float noiseScale;

texture sampleTexture;
sampler2D Texture1Sampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float distanceFromCenter = length(uv - float2(0.5, 0.5)) * 2;
    if (distanceFromCenter > 1)
        return float4(0, 0, 0, 0);
    
    float blownUpUVX = pow((abs(uv.x - 0.5)) * 2, blowUpPower);
    float blownUpUVY = pow((abs(uv.y - 0.5)) * 2, blowUpPower);
    float2 blownUpUV = float2(-blownUpUVY * blowUpSize * 0.5 + uv.x * (1 + blownUpUVY * blowUpSize), -blownUpUVX * blowUpSize * 0.5 + uv.y * (1 + blownUpUVX * blowUpSize));
    
    blownUpUV *= noiseScale;
    blownUpUV.x = (blownUpUV.x + time) % 1;
    
    float noiseColor = tex2D(Texture1Sampler, blownUpUV).r;

    noiseColor += pow(distanceFromCenter, 6);
    if (distanceFromCenter > 0.95)
        noiseColor *= (1 - ((distanceFromCenter - 0.95) / 0.05));
    
    return noiseColor * float4(lerp(shieldColor, shieldEdgeColor, pow(distanceFromCenter, shieldEdgeBlendStrenght)), shieldOpacity);
}

technique Technique1
{
    pass ShieldPass
    {
        PixelShader = compile ps_2_0 main();
    }
}