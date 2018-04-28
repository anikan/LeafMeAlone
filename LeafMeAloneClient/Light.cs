using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    class Light
    {
        /// <summary>
        /// Store the number of lights this object has
        /// </summary>
        private int NumLights;

        /// <summary>
        /// return the number of lights this object has
        /// </summary>
        /// <returns></returns>
        public int getNumLights()
        {
            return NumLights;
        }

        /// <summary>
        /// The list of lights that are stored in this object
        /// </summary>
        private List<LightParameters> lights;

        /// <summary>
        /// Light constructor. Creates a list of default point lights
        /// </summary>
        public Light(int numLights)
        {
            NumLights = numLights;

            lights = new List<LightParameters>(NumLights);

            for (int i = 0; i < NumLights; i++)
            {
                lights.Add(new LightParameters());
            }
        }

        /// <summary>
        /// Sent all light data into the shader. The shader must have an array of structs, collectively
        /// called "lights", and the array must have exactly NumLights structs. The members of the struct
        /// should be in this format:
        //  uniform extern struct LightParameters
        //        {
        //            float4 position; // also used as direction for directional light
        //            float4 intensities; // a.k.a the color of the light
        //            float4 coneDirection; // only needed for spotlights
        //
        //            float attenuation; // only needed for point and spotlights
        //            float ambientCoefficient; // how strong the light ambience should be... 0 if there's no ambience (background reflection) at all
        //            float coneAngle; // only needed for spotlights
        //            float exponent; // cosine exponent for how light tapers off
        //            int type; // specify the type of the light (directional = 0, spotlight = 2, pointlight = 1)
        //            int attenuationType; // specify the type of attenuation to use
        //            int status;         // 0 for turning off the light, 1 for turning on the light
        //            int PADDING;        // ignore this
        //
        //        } lights[NumLights];
        //
        /// </summary>
        /// <param name="shader"></param>
        public void UpdateShader(Shader shader)
        {
            shader.UseShader();
            DataStream stream = new DataStream(LightParameters.STRUCT_SIZE * NumLights, true, true);
            for (int i = 0; i < NumLights; i++)
            {
                lights[i].WriteDataToStream(stream);
            }

            stream.Position = 0;
            shader.ShaderEffect.GetVariableByName("lights")
                .SetRawValue(stream, LightParameters.STRUCT_SIZE * NumLights);
        }


    }
    
    public class LightParameters
    {
        public const int TYPE_DIRECTIONAL = 0;
        public const int TYPE_POINTLIGHT = 1;
        public const int TYPE_SPOTLIGHT = 2;
        public const int STRUCT_SIZE = sizeof(float) * 16 + sizeof(int) * 4;

        public Vector4 position; // also used as direction for directional light
        public Vector4 intensities; // a.k.a the color of the light
        public Vector4 coneDirection; // only needed for spotlights

        public float attenuation; // only needed for point and spotlights
        public float ambientCoefficient; // how strong the light ambience should be... 0 if there's no ambience (background reflection) at all
        public float coneAngle; // only needed for spotlights
        public float exponent; // cosine exponent for how light tapers off
        public int type; // specify the type of the light (directional = 0, spotlight = 2, pointlight = 1)
        public int attenuationType; // specify the type of attenuation to use
        public int status;         // 0 for turning off the light, 1 for turning on the light

        /// <summary>
        /// Constructor: by default set to a point light
        /// </summary>
        public LightParameters()
        {
            UsePointLightPreset();
        }

        /// <summary>
        /// copy from another LightParameter to this
        /// </summary>
        /// <param name="other"> copy from another light params to this </param>
        public void Copy(LightParameters other)
        {
            position.Copy(other.position);
            intensities.Copy(other.intensities);
            coneDirection.Copy(other.intensities);

            attenuation = other.attenuation;
            ambientCoefficient = other.ambientCoefficient;
            coneAngle = other.coneAngle;
            exponent = other.exponent;
            type = other.type;
            attenuationType = other.attenuationType;
            status = other.status;
        }

        public void WriteDataToStream(DataStream stream)
        {
            stream.Write(position);
            stream.Write(intensities);
            stream.Write(coneDirection);

            stream.Write(attenuation);
            stream.Write(ambientCoefficient);
            stream.Write(coneAngle);
            stream.Write(exponent);

            stream.Write(type);
            stream.Write(attenuationType);
            stream.Write(status);
            stream.Write((int)0);

            //stream.Write(stream, 0, structSize);
        }

        public void UseDirectionalPreset()
        {
            position = new Vector4(Vector3.Normalize(new Vector3(-1.0f, -1.0f, -1.0f)), 0.0f); // light direction
            intensities = new Vector4(1.0f, 0.0f, 0.0f, 0.0f); // light intensity
            coneDirection = new Vector4(0); // cone direction
            attenuation = 0.0f; // light attenuation
            ambientCoefficient = 1.0f; // ambient light coefficient
            coneAngle = 0.0f; // cone angle
            exponent = 0.0f; // spot exponent
            type = TYPE_DIRECTIONAL; // light type, set to directional
            attenuationType = 0; // attenuation type, set to constant
            status = 0; // light status, set to off
        }

        public void UsePointLightPreset()
        {
            position = new Vector4(0, 5.0f, 0, 1.0f); // light direction
            intensities = new Vector4(0.0f, 1.0f, 0.0f, 0.0f); // light intensity
            coneDirection = new Vector4(0); // cone direction
            attenuation = 0.25f; // light attenuation
            ambientCoefficient = 1.25f; // ambient light coefficient
            coneAngle = 0.0f; // cone angle
            exponent = 0.0f; // spot exponent
            type = TYPE_POINTLIGHT; // light type, set to directional
            attenuationType = 1; // attenuation type, set to constant
            status = 0; // light status, set to off
        }

        public void UseSpotLightPreset()
        {
            position = new Vector4(0, 20.0f, 0.0f, 1.0f); // light direction
            intensities = new Vector4(0.0f, 0.0f, 2.0f, 0.0f); // light intensity
            coneDirection = new Vector4(0, -1.0f, 0, 0.0f); // cone direction
            attenuation = 0.25f; // light attenuation
            ambientCoefficient = 0.55f; // ambient light coefficient
            coneAngle = 15.0f.ToRadians(); // cone angle
            exponent = 32.0f; // spot exponent
            type = TYPE_SPOTLIGHT; // light type, set to directional
            attenuationType = 1; // attenuation type, set to constant
            status = 0; // light status, set to off
        }


    }

}
