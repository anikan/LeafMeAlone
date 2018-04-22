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

        private PlayerClient ActivePlayer;
        private InputManager InputManager;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            GameClient GameClient = new GameClient();

            GraphicsRenderer.Init();

            GameClient.ActivePlayer = new PlayerClient();

            // Create an input manager for player events.
            GameClient.InputManager = new InputManager(GameClient.ActivePlayer);
            GraphicsRenderer.Form.KeyDown += GameClient.InputManager.OnKeyDown;
            GraphicsRenderer.Form.KeyUp += GameClient.InputManager.OnKeyUp;

            MessagePump.Run(GraphicsRenderer.Form, GameClient.DoGameLoop);

            GraphicsRenderer.Dispose();
        }

        private void DoGameLoop()
        {
            GraphicsRenderer.DeviceContext.ClearRenderTargetView(GraphicsRenderer.RenderTarget, new Color4(0.5f, 0.5f, 1.0f));

            // Receive any packets from the server.
            ReceivePackets();

            // Update input events.
            InputManager.Update();

            // Send any packets to the server.
            SendPackets();

            // Render on screen.
            Render();


            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);

        }

        private void Render()
        {
        }

        private void ReceivePackets()
        {

        }

        private void SendPackets()
        {

            // Create a new player packet, and fill it with player's relevant info.
            PlayerPacket playerPack = new PlayerPacket(ActivePlayer.Id);
            playerPack.Movement = ActivePlayer.MovementRequested;

            // Handy print statement to check if input is working.
            if (playerPack.Movement.X != 0 || playerPack.Movement.Y != 0)
            {
                Console.WriteLine("Movement Requested: " + playerPack.Movement);
            }


            // TODO: SEND THE ACTUAL PACKET

            // Reset the player's requested movement after the packet is sent.
            // Note: This should be last!
            ActivePlayer.ResetRequestedMovement();

        }
    }
}
