float4x4 gWorld;
float4x4 gView;
float4x4 gProj;

uniform extern Texture2D tex_diffuse;

//texture sampling
SamplerState MeshTextureSampler
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};


void VS(float4 iPosL : POSITION,
	float4 iTex : TEXTURE,
	out float4 oPosH : SV_POSITION,
	out float2 oTex : UV_TEX)
{
	float4x4 worldViewProj = mul(mul(gWorld, gView), gProj);
	oPosH = mul(iPosL, worldViewProj);
	oTex = float2(iTex.x, iTex.y);
}

float4 PS(float4 iPosH  : SV_POSITION,
	float2 iTex : UV_TEX)
	: SV_TARGET
{
	float4 ret = tex_diffuse.Sample(MeshTextureSampler, iTex);

	//need to enforce the branch is not taken before evaluation
	[branch]
	if (ret.a < .2f)
	{
		//clip all pixels getting here (dont render them)
		clip(-1);
	}
	return ret;
}




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