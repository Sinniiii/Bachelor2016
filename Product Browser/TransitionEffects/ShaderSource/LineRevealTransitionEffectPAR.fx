float progress : register(C0);
float2 lineOrigin : register(C1);
float2 lineNormal : register(C2);
float2 lineOffset : register(C3);
float fuzzyAmount : register(C4);
float2 aspectRatio : register(C5);
float2 minAspectRatio : register(C6);
float2 maxAspectRatio : register(C7);
sampler2D implicitInput : register(s0);
sampler2D oldInput : register(s1);

struct VS_OUTPUT
{
    float4 Position  : POSITION;
    float4 Color     : COLOR0;
    float2 UV        : TEXCOORD0;
};

float4 LineReveal(float2 uv)
{
	float2 currentLineOrigin = lerp(lineOrigin, lineOffset, progress);
	float2 normLineNormal = normalize(lineNormal);
	float4 c1 = tex2D(oldInput, uv);
    float4 c2 = tex2D(implicitInput, uv);
    
	float distFromLine = dot(normLineNormal, uv-currentLineOrigin);
	float p = saturate((distFromLine + fuzzyAmount) / (2.0 * fuzzyAmount));
	return lerp(c2, c1, p);
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 main(VS_OUTPUT input) : COLOR0
{
	if (input.UV.x < minAspectRatio.x || input.UV.x > maxAspectRatio.x || input.UV.y < minAspectRatio.y || input.UV.y > maxAspectRatio.y)
		return float4(0, 0, 0, 0);

	return LineReveal(float2((input.UV.x - minAspectRatio.x) / aspectRatio.x, (input.UV.y - minAspectRatio.y) / aspectRatio.y));
}

