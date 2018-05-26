// Transformation matrices
uniform float4x4 gWorld; 
uniform float4x4 gView;
uniform float4x4 gProj;

// Textures, applicable if texCount > 0
uniform Texture2D tex_diffuse;
uniform int texCount;
uniform float Shininess, Opacity;
uniform int animationIndex;

// some material properties
uniform float4 Diffuse, Specular, Ambient, Emissive;


//the tint of the object
uniform float4 Tint;

// Camera Position in object coordinates
uniform float4 CamPosObj;

// For rendering bones
uniform float4x4 boneTransforms[512];
uniform float4x4 meshTransform;

// Bone Transformation Matrices
static const int MAX_BONES_PER_GEO = 512;

// light parameters
static const int NUM_LIGHTS = 20;
static const int TYPE_DIRECTIONAL = 0;
static const int TYPE_POINT = 1;
static const int TYPE_SPOT = 2;
static const int ATTENUATION_CONSTANT = 0;
static const int ATTENUATION_LINEAR = 1;
static const int ATTENUATION_QUADRATIC = 2;
static const int STATUS_OFF = 0;
static const int STATUS_ON = 1;

// Note that in the shader, all are light parameters are in object space
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

// Vertex shader, handles the positioning of the vertices
void VS(float4 iPosL  : POSITION,
	float4 iNormL : NORMAL,
	float3 iTex : TEXTURE,
	int4 iBoneID : BONE_ID,
	float4 iBoneWeight : BONE_WEIGHT,
    out float4 oPosH  : SV_POSITION,
	out float4 oPosObj : W_POSITION,
	out float4 oNormal: NORMALS, 
	out float2 oTex : UV_TEX )
{
	float4x4 worldViewProj = mul(gWorld, mul(gView, gProj));
	float4 posBone = iPosL;
	float4 normBone = iNormL;

	if (animationIndex != -1 && iBoneWeight.x + iBoneWeight.y + iBoneWeight.z + iBoneWeight.w > 0.9f)
	{
		posBone = iBoneWeight[0] * mul(posBone, boneTransforms[iBoneID[0]])
			+ iBoneWeight[1] * mul(posBone, boneTransforms[iBoneID[1]] )
			+ iBoneWeight[2] * mul(posBone, boneTransforms[iBoneID[2]] )
			+ iBoneWeight[3] * mul(posBone, boneTransforms[iBoneID[3]] );

		normBone.w = 0.0f;
		normBone = iBoneWeight[0] * mul(normBone, boneTransforms[iBoneID[0]])
			+ iBoneWeight[1] * mul(normBone, boneTransforms[iBoneID[1]])
			+ iBoneWeight[2] * mul(normBone, boneTransforms[iBoneID[2]])
			+ iBoneWeight[3] * mul(normBone, boneTransforms[iBoneID[3]]);
	}

	posBone.w = 1.0f;
	normBone.w = 0.0f;
	oPosH = mul(posBone, worldViewProj);
	oPosObj = posBone;
	oNormal = normBone;
	oTex = float2(iTex.x, iTex.y);
}

// Pixel shader, handles coloring each pixel
float4 PS(float4 iPosHProj  : SV_POSITION, 

		float4 PositionObj : W_POSITION,
		float4 NormalObj : NORMALS, 
		float2 iTex : UV_TEX)
		: SV_TARGET
{

	float4 retColor = float4(0,0,0,1);

	float3 eVec = (float3) normalize(CamPosObj - PositionObj);
	NormalObj = normalize(NormalObj);

	for (int idx = 0; idx < NUM_LIGHTS; idx++)
	{
		if (lights[idx].status == STATUS_OFF) continue;

		float3 surfaceToLight;
		float3 lightToSurface;
		float3 rVec;
		float nDotL;
		float4 c_mat;
		float4 c_l;
		float dist;
		float attenuation_val;
		
		// find the direction of the light
		if (lights[idx].type == TYPE_DIRECTIONAL)
		{
			lightToSurface = normalize( lights[idx].position.xyz );
			surfaceToLight = -1.0f * lightToSurface;
		}
		else if (lights[idx].type == TYPE_SPOT || lights[idx].type == TYPE_POINT)
		{
			surfaceToLight = normalize(lights[idx].position.xyz - PositionObj.xyz);
			lightToSurface = -1.0f * surfaceToLight;
		}

		// find the light intensity attenuation based on the type of attenuation
		if (lights[idx].attenuationType == ATTENUATION_CONSTANT)
		{
			attenuation_val = lights[idx].attenuation;
		}
		else if (lights[idx].attenuationType == ATTENUATION_LINEAR)
		{
			attenuation_val = lights[idx].attenuation * length(lights[idx].position.xyz - PositionObj.xyz);
		}
		else if (lights[idx].attenuationType == ATTENUATION_QUADRATIC)
		{
			dist = length(lights[idx].position.xyz - PositionObj.xyz);
			attenuation_val = lights[idx].attenuation * dist * dist;
		}
		else
		{
			attenuation_val = 0.0f;
		}


		// find the angle between the normal and the light vectors
		nDotL = dot(NormalObj.xyz, surfaceToLight);

		// find the light intensity after attenuation
		if (lights[idx].type == TYPE_DIRECTIONAL || lights[idx].type == TYPE_POINT)
		{
			c_l = lights[idx].intensities / (1.0f + attenuation_val);
		}
		else if (lights[idx].type == TYPE_SPOT)
		{
			float lxd = dot(lightToSurface, normalize(lights[idx].coneDirection.xyz) );
			if (lxd <= cos(lights[idx].coneAngle))
			{
				c_l = float4(0,0,0,0);
			}
			else
			{
				c_l = float4(lights[idx].intensities.xyz * pow(lxd, lights[idx].exponent) / (1.0f + attenuation_val), 0);
			}
		}

		// find the R vector, the direction of reflected ray
		rVec = lightToSurface - (2 * dot(lightToSurface, NormalObj.xyz)) * NormalObj.xyz;
		rVec = normalize(rVec);

		// if there is no texture....
		if (texCount == 0)
		{
			c_mat = Diffuse * max(0.0f, nDotL);
			c_mat += (nDotL == 0.0f) ? float4(0,0,0,0) : Specular * max(0.0f, pow(dot(rVec, eVec), Shininess*128.0f)) * .5f;

			c_mat += Diffuse * lights[idx].ambientCoefficient * Ambient * .1f;
		}

		// else if there is a texture.... MAKE IT RED FOR NOW 
		else
		{
			c_mat = Diffuse * max(0.0f, nDotL);
			c_mat += (nDotL == 0.0f) ? float4(0,0,0,0) : Specular * max(0.0f, pow(dot(rVec, eVec), Shininess*128.0f)) * .5f;
			c_mat += Diffuse * lights[idx].ambientCoefficient * Ambient * .1f;
		}

		retColor += max( float4(0,0,0,0),  c_l * c_mat ) ;
	}

	// if there is texture
	if (texCount > 0)
	{
		// use this to find the texture color
		retColor = retColor * tex_diffuse.Sample(MeshTextureSampler, iTex);
	}

	/*if (slowBurnEnabled == 1) 
	{
		retColor.x = slowBurnColor;
	}*/
	return float4( retColor.xyz * Tint, Opacity );
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