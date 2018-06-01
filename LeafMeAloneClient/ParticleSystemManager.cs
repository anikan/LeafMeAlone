using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace Client
{
    public static class ParticleSystemManager
    {
        public static InputElement[] Elements;

        //create shader effects
        public static InputLayout InputLayout;

        public static Effect Effects;
        public static EffectPass Pass;

        public static void Init()
        {
            var btcode = ShaderBytecode.CompileFromFile(Constants.ParticleShader, "VS", "vs_4_0", ShaderFlags.None,
                EffectFlags.None);
            var btcode1 = ShaderBytecode.CompileFromFile(Constants.ParticleShader, "PS", "fx_5_0", ShaderFlags.None,
                EffectFlags.None);
            var sig = ShaderSignature.GetInputSignature(btcode);

            Effects = new Effect(GraphicsRenderer.Device, btcode1);
            EffectTechnique technique = Effects.GetTechniqueByIndex(0);
            Pass = technique.GetPassByIndex(0);

            Elements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("TEXTURE", 0, Format.R32G32B32_Float, 1),
                new InputElement("ORIGIN", 0, Format.R32G32B32_Float, 2)
            };

            InputLayout = new InputLayout(GraphicsRenderer.Device, sig, Elements);
        }
        
    }
}
