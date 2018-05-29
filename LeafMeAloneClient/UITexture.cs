using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;
using MapFlags = SlimDX.Direct3D11.MapFlags;

namespace Client
{
    class UITexture
    {
        //create buffers
        private SlimDX.Direct3D11.Buffer VBO_Verts, VBO_Tex;
        private Buffer EBO;
        private DataStream Tex, Faces;

        private Transform modelMatrix;

        //create texture
        private ShaderResourceView TexSRV;

        private readonly int size = Vector3.SizeInBytes * 4;
        public UITexture(string TexturePath)
        {
            modelMatrix = new Transform();
            Faces = new DataStream(sizeof(uint) * 6, true, true);
            Tex = new DataStream(size, true, true);

            var vert_desc = new BufferDescription(size, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            VBO_Verts = new Buffer(GraphicsRenderer.Device, vert_desc);

            Tex.Write(new Vector3(0f, 0f, 0f));
            Tex.Write(new Vector3(1f, 1f, 0f));
            Tex.Write(new Vector3(0f, 1f, 0f));
            Tex.Write(new Vector3(1f, 0f, 0f));

            Faces.Write((uint)4);
            Faces.Write((uint)(4 + 2));
            Faces.Write((uint)(4 + 1));
            Faces.Write((uint)4);
            Faces.Write((uint)(4 + 1));
            Faces.Write((uint)(4 + 3));

            Faces.Position = 0;
            Tex.Position = 0;

            VBO_Tex = new Buffer(GraphicsRenderer.Device, Tex, size, ResourceUsage.Immutable, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            EBO = new Buffer(GraphicsRenderer.Device, Faces, 6 * sizeof(uint), ResourceUsage.Immutable, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            TexSRV = Utility.CreateTexture(TexturePath,GraphicsRenderer.Device);
        }

        public void Update()
        {
            var vertBox = GraphicsRenderer.DeviceContext.MapSubresource(VBO_Verts, 0, MapMode.WriteDiscard, MapFlags.None);

            float objSize = 1.0f;
            Vector3 posVal = Vector3.Zero;
            Vector3 topLeft_Both = new Vector3(posVal.X - objSize, posVal.Y + objSize, posVal.Z);
            Vector3 bottomRight_Both = new Vector3(posVal.X + objSize, posVal.Y - objSize, posVal.Z);
            Vector3 topRight = new Vector3(posVal.X + objSize, posVal.Y + objSize, posVal.Z);
            Vector3 bottomLeft = new Vector3(posVal.X - objSize, posVal.Y - objSize, posVal.Z);

            vertBox.Data.Write(topLeft_Both);
            vertBox.Data.Write(bottomRight_Both);
            vertBox.Data.Write(topRight);
            vertBox.Data.Write(bottomLeft);

            GraphicsRenderer.DeviceContext.UnmapSubresource(VBO_Verts, 0);
        }

        public void Draw()
        {

            GraphicsRenderer.DeviceContext.InputAssembler.InputLayout = UITextureManager.InputLayout;
            GraphicsRenderer.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VBO_Verts, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(VBO_Tex, Vector3.SizeInBytes, 0));
            GraphicsRenderer.DeviceContext.InputAssembler.SetIndexBuffer(EBO, Format.R32_UInt, 0);
            
            UITextureManager.Effects.GetVariableByName("gWorldViewProj").AsMatrix().SetMatrix(modelMatrix.AsMatrix() * GraphicsManager.ActiveCamera.m_ViewMatrix * GraphicsRenderer.ProjectionMatrix);
            UITextureManager.Effects.GetVariableByName("tex_diffuse").AsResource().SetResource(TexSRV);

            //set blend state
            GraphicsRenderer.DeviceContext.OutputMerger.BlendState = GraphicsRenderer.BlendState;

            //turn off depth for now
            GraphicsRenderer.DeviceContext.OutputMerger.DepthStencilState = GraphicsRenderer.DepthStateOff;

            //apply pass
            UITextureManager.Pass.Apply(GraphicsRenderer.Device.ImmediateContext);
            GraphicsRenderer.DeviceContext.DrawIndexed(6, 0, 0);

            //turn back on depth 
            GraphicsRenderer.DeviceContext.OutputMerger.DepthStencilState = GraphicsRenderer.DepthState;
            GraphicsRenderer.DeviceContext.OutputMerger.BlendState = null;
        }
    }
    public static class UITextureManager
    {
        public static InputElement[] Elements;

        //create shader effects
        public static InputLayout InputLayout;

        public static Effect Effects;
        public static EffectPass Pass;

        public static void Init()
        {
            var btcode = ShaderBytecode.CompileFromFile(Constants.UIShader, "VS", "vs_4_0", ShaderFlags.None,
                EffectFlags.None);
            var btcode1 = ShaderBytecode.CompileFromFile(Constants.UIShader, "PS", "fx_5_0", ShaderFlags.None,
                EffectFlags.None);
            var sig = ShaderSignature.GetInputSignature(btcode);

            Effects = new Effect(GraphicsRenderer.Device, btcode1);
            EffectTechnique technique = Effects.GetTechniqueByIndex(0);
            Pass = technique.GetPassByIndex(0);

            Elements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("TEXTURE", 0, Format.R32G32B32_Float, 1)
            };

            InputLayout = new InputLayout(GraphicsRenderer.Device, sig, Elements);
        }





    }
}
