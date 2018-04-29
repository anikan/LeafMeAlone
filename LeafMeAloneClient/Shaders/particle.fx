float4x4 gWorld;
float4x4 gView;
float4x4 gProj;

uniform extern Texture2D tex_diffuse;

SamplerState MeshTextureSampler
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

void VS(float4 iPosL  : POSITION,
	float4 iColor : COLOR0,
	out float4 oPosH : SV_POSITION,
	out float4 oColor : COLOR0)
{
	float4x4 worldViewProj = mul(mul(gWorld, gView), gProj);
	oPosH = mul(iPosL, worldViewProj);
	oColor = iColor;
}


float4 PS(float4 iPosH  : SV_POSITION, float4 iColor : COLOR0) : SV_TARGET
{
	return iColor;
}


//float VS_Text(float iPosL : Position,
//	float3 iTex : TEXTURE,
//	out float oPosH : SV_POSITION,
//	out float2 oTex : UV_TEX)
//{
//	float4x4 worldViewProj = mul(mul(gWorld, gView), gProj);
//	oPosH = mul(iPosL, worldViewProj);
//	//oNormal = mul(iNormL, gWorld);
//	oTex = float2(iTex.x, iTex.y);
//}

//float4 PS_TEXT(float4 iPosH  : SV_POSITION,
//	float2 iTex : UV_TEX)
//	: SV_TARGET
//{
//float4 retColor;
//
//float3 AmbColor = float3(.1, .1, .8);
//float3 LightDirection = normalize(float3(-2, 2, -.01));
//float3 LightColor = float3(1, 1, 1);
//float3 DiffuseColor = float3(0.5, 0.5, 0.5);
//
//// Compute irradiance (sum of ambient & direct lighting)
//float3 irradiance = AmbColor + LightColor * max(0,dot(LightDirection,float4(1,1,1,1)));
//
//// Diffuse reflectance
//float3 reflectance = irradiance * DiffuseColor;
//
//// Gamma correction
//retColor = float4(sqrt(reflectance), 1);
//
//// if there is texture
//if (texCount > 0)
//{
//	// use this to find the texture color
//	retColor = retColor * tex_diffuse.Sample(MeshTextureSampler, iTex);
//}
//return retColor;
//}




//void GS(point 

technique10 ColorTech
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, PS()));
	}
}