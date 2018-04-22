float4x4 gWorld; 
float4x4 gView; 
float4x4 gProj;

void VS(float4 iPosL  : POSITION,
		float4 iNormL : NORMAL,
        out float4 oPosH  : SV_POSITION,
		out float4 oNormal: NORMAL )
{
	float4x4 worldViewProj = mul(mul(gWorld, gView), gProj);
	oPosH = mul(iPosL, worldViewProj);
	oNormal = mul(iNormL,gWorld);
}

float4 PS(float4 iPosH  : SV_POSITION, float4 iNormL : NORMAL) : SV_TARGET
{
	float3 AmbColor = float3(.1,.1,.8);
    float3 LightDirection=normalize(float3(-2,2,-.01));
	float3 LightColor=float3(1,1,1);
	float3 DiffuseColor=float3(0.5,0.5,0.5);
	
	// Compute irradiance (sum of ambient & direct lighting)
	float3 irradiance=AmbColor + LightColor * max(0,dot(LightDirection,iNormL));

	// Diffuse reflectance
	float3 reflectance=irradiance * DiffuseColor;

	// Gamma correction
	return float4(sqrt(reflectance),1);
	
}

technique10 ColorTech
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_4_0, VS()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_4_0, PS()));
    }
}	