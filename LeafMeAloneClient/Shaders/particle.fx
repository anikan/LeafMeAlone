uniform extern float4x4 gWorldViewProj;
uniform extern float3 gOrigin;
uniform extern float CutoffSpeed;
uniform extern float CutoffDist;
uniform extern float StopDist;

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
	out float4 oPosObj : POSITION_OBJ,
	out float2 oTex : UV_TEX)
{
	oPosH = mul(iPosL, gWorldViewProj);
	oPosObj = iPosL;
	oTex = float2(iTex.x, iTex.y);
}

float4 PS(float4 iPosH  : SV_POSITION,
	float4 iPosObj : POSITION_OBJ,
	float2 iTex : UV_TEX)
	: SV_TARGET
{
	float4 ret = tex_diffuse.Sample(MeshTextureSampler, iTex);

	float dist = length(iPosObj.xyz - gOrigin);
	float factor = 1.0f;
	if (dist > CutoffDist)
	{
		factor = 1.0f / (1.0f + CutoffSpeed * (dist-CutoffDist));
	}
	
	//ret = factor * ret;

	//need to enforce the branch is not taken before evaluation
	[branch]
	if (ret.a < .09f)
	{
		//clip all pixels getting here (dont render them)
		clip(-1);
	}
	return factor*ret;
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