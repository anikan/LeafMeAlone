using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D11;

namespace Client
{
    /// <summary>
    /// used for storing information about the material properties of a mesh
    /// </summary>
    public class MyMaterial
    {
        public Vector4 diffuse;
        public Vector4 ambient;
        public Vector4 specular;
        public Vector4 emissive;
        public ShaderResourceView texSRV;
        public float shininess;
        public float opacity;
        public int texCount; // will be 0 if there is no texture

        /// <summary>
        /// set texCount, which tells us if a texture is loaded or not
        /// </summary>
        /// <param name="t"></param>
        public void setTexCount(int t)
        {
            texCount = t;
        }

        /// <summary>
        /// Sets the diffuse components of the material property
        /// </summary>
        /// <param name="x"> red </param>
        /// <param name="y"> green </param>
        /// <param name="z"> blue </param>
        /// <param name="w"> alpha </param>
        public void setDiffuse(float x, float y, float z, float w)
        {
            diffuse = new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Sets the ambient components of the material property
        /// </summary>
        /// <param name="x"> red </param>
        /// <param name="y"> green </param>
        /// <param name="z"> blue </param>
        /// <param name="w"> alpha </param>
        public void setAmbient(float x, float y, float z, float w)
        {
            ambient = new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Sets the specular components of the material property
        /// </summary>
        /// <param name="x"> red </param>
        /// <param name="y"> green </param>
        /// <param name="z"> blue </param>
        /// <param name="w"> alpha </param>
        public void setSpecular(float x, float y, float z, float w)
        {
            specular = new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Sets the emissive components of the material property
        /// </summary>
        /// <param name="x"> red </param>
        /// <param name="y"> green </param>
        /// <param name="z"> blue </param>
        /// <param name="w"> alpha </param>
        public void setEmissive(float x, float y, float z, float w)
        {
            emissive = new Vector4(x, y, z, w);
        }

        /// <summary>
        /// sets the shininess of the material
        /// </summary>
        /// <param name="x"> shininess </param>
        public void setShininess(float x)
        {
            shininess = x;
        }

        /// <summary>
        /// sets the opacity of the material; only usable if blending is enabled
        /// </summary>
        /// <param name="x"> opacity </param>
        public void setOpacity(float x)
        {
            opacity = x;
        }

        /// <summary>
        /// sets the texture of the material
        /// </summary>
        /// <param name="tex"> texture </param>
        public void setDiffuseTexture(ShaderResourceView tex)
        {
            texSRV = tex;
        }
    }
    
}
