using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client;
using LeafMeAloneClient;
using Shared;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Windows;


namespace Client
{
    class GameClient
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        private NetworkClient networkClient = new NetworkClient();

        private static void Main()
        {
            GameClient gameClient = new GameClient();

            GraphicsRenderer.Init();
            MessagePump.Run(GraphicsRenderer.Form, gameClient.DoGameLoop);
            GraphicsRenderer.Dispose();

        }

        private void DoGameLoop()
        {
            GraphicsRenderer.DeviceContext.ClearRenderTargetView(GraphicsRenderer.RenderTarget, new Color4(0.5f, 0.5f, 1.0f));
            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);

            // Send test data to the remote device.  
            networkClient.Send("This is a test<EOF>");

            // Receive the response from the remote device.  
            networkClient.Receive();

            // Write the response to the console.  
            Console.WriteLine("Response received : {0}", networkClient.response);
        }

        public GameClient()
        {
            networkClient.StartClient();

        }

    }
}
