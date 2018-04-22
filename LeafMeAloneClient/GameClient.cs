using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client;
using LeafMeAloneClient;
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
        
        private Camera Camera => GraphicsManager.ActiveCamera;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        private NetworkClient networkClient = new NetworkClient();

        private static void Main()
        {
            GameClient GameClient = new GameClient();

            GraphicsRenderer.Init();

            GameClient.ActivePlayer = new PlayerClient();
           // gameClient.cockleModel = new Model(@"../../model-cockle/common-cockle.obj");
            //gameClient.cockleModel.m_Properties.Scale = new Vector3(0.5f, 0.5f, 0.5f);
            //gameClient.cockleModel.m_Properties.Position = new Vector3(0f, -10.0f, 0f);


            GraphicsManager.ActiveCamera = new Camera(new Vector3(0, 0, -10), Vector3.Zero, Vector3.UnitY);

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

            // Receive the response from the remote device.  
            networkClient.Receive();

            // Write the response to the console.  
            //Console.WriteLine("Response received : {0}", networkClient.response);

            GraphicsRenderer.DeviceContext.ClearDepthStencilView(GraphicsRenderer.DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            ReceivePackets();

            // Update input events.
            InputManager.Update();

            // Send any packets to the server.
            SendPackets();

            GraphicsManager.ActiveCamera.RotateCamera(new Vector3(0,0,0), new Vector3(1,0,0), 0.0001f);

            Render();

            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);

        }

        public GameClient()
        {
            networkClient.StartClient();
        }

        private void Render()
        {
            ActivePlayer.Update();
            ActivePlayer.Draw();
        }

        private void ReceivePackets()
        {

        }

        private void SendPackets()
        {
            PlayerPacket toSend = ClientPacketFactory.CreatePacket(ActivePlayer);
            byte[] data = PlayerPacket.Serialize(toSend);
            networkClient.Send(data);

            // Handy print statement to check if input is working.
            if (toSend.MovementX != 0 || toSend.MovementY != 0)
            {
                Console.WriteLine("Movement Requested: " + toSend.MovementX + "," + toSend.MovementY);
            }

            // Reset the player's requested movement after the packet is sent.
            // Note: This should be last!
            ActivePlayer.ResetRequestedMovement();
        }

    }
}
