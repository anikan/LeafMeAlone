using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client;
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
        private static void Main()
        {
            GameClient gameClient = new GameClient();

            GraphicsRenderer.Init();

            // Create an input manager for player events.
            InputManager inputManager = new InputManager(new Player(Vector2.Zero));

            // Add the key press input handler to call our InputManager directly.
            GraphicsRenderer.Form.KeyPress += inputManager.OnKeyPress;

            MessagePump.Run(GraphicsRenderer.Form, gameClient.DoGameLoop);

            GraphicsRenderer.Dispose();
        }

        private void DoGameLoop()
        {
            GraphicsRenderer.DeviceContext.ClearRenderTargetView(GraphicsRenderer.RenderTarget, new Color4(0.5f, 0.5f, 1.0f));
            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);
        }
    }
}
