// Transformation matrices
uniform extern float4x4 gWorld; 
uniform extern float4x4 gView;
uniform extern float4x4 gProj;

// Textures, applicable if texCount > 0
uniform extern Texture2D tex_diffuse;
uniform extern int texCount;

// some material properties
uniform extern float4 Diffuse, Specular, Ambient, Emissive;
uniform extern float Shininess, Opacity;

// light parameters
static const int NUM_LIGHTS = 20;


uniform extern struct LightParameters
{
	float4 position; // also used as direction for directional light
	float4 intensities; // a.k.a the color of the light
	float4 coneDirection; // only needed for spotlights

	float attenuation; // only needed for point and spotlights
	float ambientCoefficient; // how strong the light ambience should be... 0 if there's no ambience (background reflection) at all
	float coneAngle; // only needed for spotlights
	float exponent; // cosine exponent for how light tapers off
	int type; // specify the type of the light (directional = 0, spotlight = 2, pointlight = 1)
	int attenuationType; // specify the type of attenuation to use
	int status;         // 0 for turning off the light, 1 for turning on the light
	int PADDING;		// ignore this

} lights[NUM_LIGHTS];

SamplerState MeshTextureSampler
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

void VS(float4 iPosL  : POSITION,
		float4 iNormL : NORMAL,
		float3 iTex : TEXTURE,
        out float4 oPosH  : SV_POSITION,
		out float4 oNormal: NORMALPS, 
		out float2 oTex : UV_TEX )
{
	float4x4 worldViewProj = mul(mul(gWorld, gView), gProj);
	oPosH = mul(iPosL, worldViewProj);
	oNormal = mul(iNormL,gWorld);
	oTex = float2(iTex.x, iTex.y);
}

float4 PS(float4 iPosH  : SV_POSITION, 
		float4 iNormL : NORMALPS, 
		float2 iTex : UV_TEX)
		: SV_TARGET
{
	float4 retColor;

    float3 LightDirection=normalize(float3(-2,2,-.01));
	float3 LightColor=float3(1,1,1);

	float3 AmbColor = float3(Ambient.xyz);
	float3 DiffuseColor=float3( Diffuse.xyz );
	
	// Compute irradiance (sum of ambient & direct lighting)
	float3 irradiance=AmbColor + LightColor * max(0,dot(LightDirection,iNormL));

	// Diffuse reflectance
	float3 reflectance=irradiance * DiffuseColor;

	// Gamma correction
	retColor = float4(sqrt(reflectance), 1);

	// if there is texture
	if (texCount > 0)
	{
		// use this to find the texture color
		retColor = retColor * tex_diffuse.Sample(MeshTextureSampler, iTex);
	}


	return retColor;
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