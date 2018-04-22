float4x4 gWorld; 
float4x4 gView; 
float4x4 gProj;

float4 VShader(float4 position : POSITION) : SV_POSITION
{
	float4x4 worldViewProj = mul(mul(gWorld, gView), gProj);
	return mul(position, worldViewProj);
}

float4 PShader(float4 position : SV_POSITION) : SV_Target
{
	return float4(1.0f, 1.0f, 0.0f, 1.0f);
}

technique10 ColorTech
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_4_0, VShader()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_4_0, PShader()));
    }
}	