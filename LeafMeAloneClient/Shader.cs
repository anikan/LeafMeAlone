using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace Client
{
    public class Shader
    {
        //private InputElement[] Elements;
        private InputLayout Layout;
        public Effect ShaderEffect;
        public EffectPass ShaderPass;
     
        // pass in the filepath to the shader, the vertex shader function name, and pixel shader function name
        public Shader(string p_filepath, string p_VS_name, string p_PS_name, InputElement[] p_elements)
        {
            LoadShader(p_filepath, p_VS_name, p_PS_name, p_elements);
        }

        // uses default name for the VS/PS functions
        public Shader(string p_filepath, InputElement[] p_elements)
        {
            string VS_name = "VS";
            string PS_name = "PS";

            LoadShader(p_filepath, VS_name, PS_name, p_elements);
        }

        // uses default element layout and VS/PS functions
        public Shader(string p_filepath)
        {
            InputElement[] elem = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 1),
                new InputElement("TEXTURE", 0, Format.R32G32B32_Float, 2),
                new InputElement("BONE_ID", 0, Format.R32G32B32A32_SInt, 3),
                new InputElement("BONE_WEIGHT", 0, Format.R32G32B32A32_Float, 4)
            };

            string VS_name = "VS";
            string PS_name = "PS";

            LoadShader(p_filepath, VS_name, PS_name, elem);
        }

        private void LoadShader(string p_filepath, string p_VS_name, string p_PS_name, InputElement[] p_elements)
        {
            var vshader = ShaderBytecode.CompileFromFile(p_filepath, p_VS_name, "vs_4_0", ShaderFlags.None,
                EffectFlags.None);
            var pshader = ShaderBytecode.CompileFromFile(p_filepath, p_PS_name, "fx_5_0", ShaderFlags.None,
                EffectFlags.None);
            var sig = ShaderSignature.GetInputSignature(vshader);

            ShaderEffect = new Effect(GraphicsRenderer.Device, pshader);
            EffectTechnique technique = ShaderEffect.GetTechniqueByIndex(0);
            ShaderPass = technique.GetPassByIndex(0);

            Layout = new InputLayout(GraphicsRenderer.Device, sig, p_elements);
        }

        public void UseShader()
        {
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.InputLayout = Layout;
            GraphicsRenderer.Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            GraphicsManager.ActiveShader = this;
        }
    }
}
