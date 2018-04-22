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
        private PlayerClient activePlayer;

        private Camera Camera => GraphicsManager.ActiveCamera;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            GameClient gameClient = new GameClient();

            GraphicsRenderer.Init();

            gameClient.activePlayer = new PlayerClient();
            GraphicsManager.ActiveCamera = new Camera(new Vector3(0, 0, -10), Vector3.Zero, Vector3.UnitY);

            // Create an input manager for player events.
            InputManager inputManager = new InputManager(gameClient.activePlayer);

            // Add the key press input handler to call our InputManager directly.
            GraphicsRenderer.Form.KeyPress += inputManager.OnKeyPress;

            MessagePump.Run(GraphicsRenderer.Form, gameClient.DoGameLoop);

            GraphicsRenderer.Dispose();


        }

        private void DoGameLoop()
        {
            GraphicsRenderer.DeviceContext.ClearRenderTargetView(GraphicsRenderer.RenderTarget, new Color4(0.5f, 0.5f, 1.0f));
            ReceivePackets();
            SendPackets();

            GraphicsManager.ActiveCamera.RotateCamera(new Vector3(0,0,0), new Vector3(1,0,0), 0.01f);

            Render();
            activePlayer.ResetTransientState();
            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);

        }

        private void Render()
        {
            activePlayer.Update();
            activePlayer.Draw();
        }

        private void ReceivePackets()
        {

        }

        private void SendPackets()
        {
            PlayerPacket playerPack = new PlayerPacket(activePlayer.GetId());
            playerPack.Movement = activePlayer.MovementRequested;
        }

    }
}
