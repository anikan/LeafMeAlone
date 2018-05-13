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
        ///  uniform extern struct LightParameters
        ///        {
        ///            float4 position : also used as direction for directional light
        ///            float4 intensities : a.k.a the color of the light
        ///            float4 coneDirection : only needed for spotlights
        ///
        ///            float attenuation : only needed for point and spotlights
        ///            float ambientCoefficient : how strong the light ambience should be... 0 if there's no ambience (background reflection) at all
        ///            float coneAngle : only needed for spotlights
        ///            float exponent : cosine exponent for how light tapers off
        ///            int type : specify the type of the light (directional = 0, spotlight = 2, pointlight = 1)
        ///            int attenuationType : specify the type of attenuation to use
        ///            int status : 0 for turning off the light, 1 for turning on the light
        ///            int PADDIND : ignore this
        ///
        ///        } lights[NumLights];
        ///
        /// </summary>
        /// <param name="shader"> the shader of the mode </param>
        /// <param name="model"> the model matrix of the model </param>
        public void UpdateShader(Shader shader, Matrix model)
        {
            //shader.UseShader();
            DataStream stream = new DataStream(LightParameters.STRUCT_SIZE * NumLights, true, true);
            for (int i = 0; i < NumLights; i++)
            {
                lights[i].WriteDataToStream(stream, model);
            }

            stream.Position = 0;
            shader.ShaderEffect.GetVariableByName("lights")
                .SetRawValue(stream, LightParameters.STRUCT_SIZE * NumLights);
        }

        /// <summary>
        /// Return the Light Parameters at index i
        /// Use this to read light properties, and its possible to directly
        /// change the light properties using the pointer returned
        /// </summary>
        /// <param name="i"> returns the i-th light parameters of the instance </param>
        /// <returns></returns>
        public LightParameters GetLightParameters(int i)
        {
            if (i < NumLights)
            {
                return lights[i];
            }
            else throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Copies over the light properties of the newLightProperties into the i-th 
        /// light parameters of the class 
        /// </summary>
        /// <param name="i"> copies into the i-th light parameters of the instance </param>
        /// <param name="newLightProperties"> the properties to be copied </param>
        public void CopyLightParameters(int i, LightParameters newLightProperties)
        {
            if (i < NumLights)
            {
                lights[i].Copy( newLightProperties );
            }
            else throw new IndexOutOfRangeException();
        }


    }
    
    public class LightParameters
    {
        // some constants that describe the states of the lights
        public const int TYPE_DIRECTIONAL = 0;
        public const int TYPE_POINTLIGHT = 1;
        public const int TYPE_SPOTLIGHT = 2;

        public const int ATTENUATION_CONSTANT = 0;
        public const int ATTENUATION_LINEAR = 1;
        public const int ATTENUATION_QUADRATIC = 2;

        public const int STATUS_OFF = 0;
        public const int STATUS_ON = 1;

        // struct size of light parameters in a corresponding shader
        public const int STRUCT_SIZE = sizeof(float) * 16 + sizeof(int) * 4;

        // actual light parameters
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

        /// <summary>
        /// Write data into the stream, by converting it into the object space first 
        /// </summary>
        /// <param name="stream"></param>
        public void WriteDataToStream(DataStream stream, Matrix toWorld)
        {
            // make sure that transformations don't get mistaken
            if (type == TYPE_DIRECTIONAL) position.W = 0;
            
            Matrix toObj = Matrix.Invert(toWorld);
            Vector4 pos_obj = Vector4.Transform(position, toObj) ;

            Vector4 conePos = -1.0f * coneDirection;
            conePos.W = 1.0f;
            conePos = Vector4.Transform(conePos, toObj);
            Vector4 coneDir_obj = Vector4.Normalize(-1.0f * conePos);
            coneDir_obj.W = 0;

            stream.Write(pos_obj);
            stream.Write(intensities);
            stream.Write(coneDir_obj);

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

        /// <summary>
        /// Make the current light a directional light that uses some preset parameters
        /// </summary>
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
            attenuationType = ATTENUATION_CONSTANT; // attenuation type, set to constant
            status = STATUS_OFF; // light status, set to off
        }

        /// <summary>
        /// Make the current light a point light that uses some preset parameters
        /// </summary>
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
            attenuationType = ATTENUATION_LINEAR; // attenuation type, set to constant
            status = STATUS_OFF; // light status, set to off
        }

        /// <summary>
        /// Make the current light a spot light that uses some preset parameters
        /// </summary>
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
            attenuationType = ATTENUATION_LINEAR; // attenuation type, set to constant
            status = STATUS_OFF; // light status, set to off
        }


    }

}
