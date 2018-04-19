using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client;
using Shared;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.DXGI.Device;


namespace Client
{
    class GameClient
    {
        private static Camera Camera;
        private static Model testModel;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {

            GraphicsRenderer.Init();
            Camera = new Camera(new Vector3(0, 0, -10), Vector3.Zero, Vector3.UnitY);
            GraphicsManager.ActiveCamera = Camera;
            testModel = new Model("C:\\Users\\CSVR\\Desktop\\CSE125\\LeafMeAlone\\LeafMeAloneClient\\Pants14Triangles.fbx",new Shader());

            MessagePump.Run(GraphicsRenderer.Form, DoGameLoop);
            GraphicsRenderer.Dispose();
        }


        /// <summary>
        /// The Main Game Loop
        /// </summary>
        private static void DoGameLoop()
        {
            GraphicsRenderer.DeviceContext.ClearRenderTargetView(GraphicsRenderer.RenderTarget, new Color4(0.5f, 0.5f, 1.0f));
            testModel.Update();
            testModel.Draw();


            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);
        }

    }
}
