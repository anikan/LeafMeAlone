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
using System.Diagnostics;


namespace Client
{
    class GameClient
    {

        private PlayerClient ActivePlayer;
        private InputManager InputManager;

        private Camera Camera => GraphicsManager.ActiveCamera;

        public List<LeafClient> leaves;

        public Stopwatch Timer;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        private NetworkClient networkClient = new NetworkClient();

        private static void Main()
        {


            GameClient Client = new GameClient();
            Client.Timer = new Stopwatch();

            GraphicsRenderer.Init();

            Client.ActivePlayer = new PlayerClient();

            GraphicsManager.ActiveCamera = new Camera(new Vector3(0, 50, -30), Vector3.Zero, Vector3.UnitY);
            GraphicsManager.ActivePlayer = Client.ActivePlayer;

            Client.leaves = new List<LeafClient>();

            for (int x = -5; x < 5; x++)
            {
                for (int y = -5; y < 5; y++)
                {

                    LeafClient newLeaf = new LeafClient();
                    newLeaf.Transform.Position = new Vector3(x * 5, 0.0f, y * 5);
                    Client.leaves.Add(newLeaf);


                }
            }

            // Set up the input manager.
            Client.SetupInputManager();

            GraphicsRenderer.Form.KeyDown += Client.InputManager.OnKeyDown;
            GraphicsRenderer.Form.KeyUp += Client.InputManager.OnKeyUp;

            //TODO FOR TESTING ONLY
            //GraphicsRenderer.Form.KeyDown += TestPlayerMovementWithoutNetworking;

            MessagePump.Run(GraphicsRenderer.Form, Client.DoGameLoop);

            GraphicsRenderer.Dispose();

        }


        public static void TestPlayerMovementWithoutNetworking(object ignore, KeyEventArgs keyArg)
        {
            if (keyArg.KeyCode == Keys.Up)
            {
                GraphicsManager.ActivePlayer.Transform.Position += Vector3.UnitY;
            }
            if (keyArg.KeyCode == Keys.Down)
            {
                GraphicsManager.ActivePlayer.Transform.Position -= Vector3.UnitY;
            }
            if (keyArg.KeyCode == Keys.Left)
            {
                GraphicsManager.ActivePlayer.Transform.Position += Vector3.UnitX;
            }
            if (keyArg.KeyCode == Keys.Right)
            {
                GraphicsManager.ActivePlayer.Transform.Position -= Vector3.UnitX;
            }
            if (keyArg.KeyCode == Keys.Space)
            {
                GraphicsManager.ActivePlayer.Transform.Rotation += Vector3.UnitY * 20;
            }


        }
        private void DoGameLoop()
        {
            GraphicsRenderer.DeviceContext.ClearRenderTargetView(GraphicsRenderer.RenderTarget, new Color4(0.5f, 0.5f, 1.0f));

            GraphicsRenderer.DeviceContext.ClearDepthStencilView(GraphicsRenderer.DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            ReceivePackets();

            // Update input events.
            InputManager.Update();

            // Send any packets to the server.
            SendPackets();

            // GraphicsManager.ActiveCamera.RotateCamera(new Vector3(0, 0, 0), new Vector3(1, 0, 0), 0.0001f);
            Update();

            Render();

            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);

        }

        public GameClient()
        {
            networkClient.StartClient();
            
            // Receive the response from the remote device.  
            //networkClient.Receive();
        }

        private void Update()
        {
            float delta = Timer.ElapsedMilliseconds;

            ActivePlayer.Update(delta);

            for (int i = 0; i < leaves.Count; i++)
            {
                leaves[i].Update(delta);
            }

            Timer.Restart();


        }

        private void Render()
        {

       
            ActivePlayer.Draw();

            for (int i = 0; i < leaves.Count; i++)
            {
                leaves[i].Draw();
            }


        }

        /// <summary>
        /// Recieves packets from the server, updates the player
        /// TODO: Make this hash the first 4 bytes into an object ID
        /// </summary>
        private void ReceivePackets()
        {
            // Receive the response from the remote device.  
            networkClient.Receive();

            for (int i = 0; i < networkClient.PlayerPackets.Count(); i++)
            {
                ActivePlayer.UpdateFromPacket(networkClient.PlayerPackets[i]);
            }

            networkClient.PlayerPackets.Clear();
        }

        /// <summary>

        /// Sends out the data associated with the active player's input, resets requested movement
        /// </summary>
        private void SendPackets()
        {
            // Create a new player packet, and fill it with player's relevant info.
            PlayerPacket toSend = ClientPacketFactory.CreatePacket(ActivePlayer);
            byte[] data = PlayerPacket.Serialize(toSend);
            networkClient.Send(data);
            
            // COMMENT OUT WHEN SERVER IS INTEGRATED
            /*ActivePlayer.Transform.Position = new Vector3(ActivePlayer.Transform.Position.X - playerPack.Movement.X * 0.01f,
                                                          ActivePlayer.Transform.Position.Y,
                                                          ActivePlayer.Transform.Position.Z - playerPack.Movement.Y * 0.01f );*/
        
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
            GraphicsRenderer.Form.MouseMove += InputManager.OnMouseMove;
        }

    }
}
