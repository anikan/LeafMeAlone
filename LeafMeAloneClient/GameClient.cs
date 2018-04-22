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

        private PlayerClient ActivePlayer;
        private InputManager InputManager;

        private static Camera Camera;
        private static Model testModel;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            GameClient Client = new GameClient();

            GraphicsRenderer.Init();

            Client.ActivePlayer = new PlayerClient();

            // Set up the input manager.
            Client.SetupInputManager();

            Camera = new Camera(new Vector3(0, 0, -10), Vector3.Zero, Vector3.UnitY);
            GraphicsManager.ActiveCamera = Camera;
            testModel = new Model(@"../../Pants14Triangles.fbx");

            MessagePump.Run(GraphicsRenderer.Form, Client.DoGameLoop);

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
            testModel.Update();
            testModel.Draw();
        }

        private void ReceivePackets()
        {

        }

        /// <summary>
        /// Sends all packets this frame.
        /// </summary>
        private void SendPackets()
        {

            // Create a new player packet, and fill it with player's relevant info.
            PlayerPacket playerPack = new PlayerPacket();

            playerPack.Movement = ActivePlayer.MovementRequested;
            playerPack.UsingToolPrimary = ActivePlayer.UseToolPrimaryRequest;
            playerPack.UsingToolSecondary = ActivePlayer.UseToolSecondaryRequest;

            Console.WriteLine(playerPack.ToString());

            // TODO: SEND THE ACTUAL PACKET

            // Reset the player's requested movement after the packet is sent.
            // Note: This should be last!
            ActivePlayer.ResetRequests();

        }

        /// <summary>
        /// Sets up the input manager and relevant input events.
        /// </summary>
        private void SetupInputManager()
        {

            // Create an input manager for player events.
            InputManager = new InputManager(ActivePlayer);

            // Input events for the input manager.
            GraphicsRenderer.Form.KeyDown += InputManager.OnKeyDown;
            GraphicsRenderer.Form.KeyUp += InputManager.OnKeyUp;
            GraphicsRenderer.Form.MouseDown += InputManager.OnMouseDown;
            GraphicsRenderer.Form.MouseUp += InputManager.OnMouseUp;
        }

    }
}
