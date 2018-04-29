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
        private Dictionary<int, GameObjectClient> gameObjects;

        private Camera Camera => GraphicsManager.ActiveCamera;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        private NetworkClient networkClient = new NetworkClient();

        private static void Main()
        {
            GameClient Client = new GameClient();

            GraphicsRenderer.Init();


            GraphicsManager.ActiveCamera = new Camera(new Vector3(0, 50, -30), Vector3.Zero, Vector3.UnitY);
            GraphicsManager.ActivePlayer = Client.ActivePlayer;


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

            if (ActivePlayer != null)
            {
                // Update input events.
                InputManager.Update();
                // Send any packets to the server.
                SendPackets();

            }

            //GraphicsManager.ActiveCamera.RotateCamera(
            // new Vector3(0, 0, 0), new Vector3(1, 0, 0), 0.0001f);
            Render();


            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);

        }

        public GameClient()
        {
            networkClient.StartClient();

            // Receive the response from the remote device.  
            //networkClient.Receive();
        }

        /// <summary>
        /// Loops through the hashtable of gameobjects and draws them
        /// </summary>
        private void Render()
        {
            foreach (KeyValuePair<int, GameObjectClient> kv in
                gameObjects.AsEnumerable()) {
                GameObjectClient gameObject = kv.Value;
                gameObject.Update();
                gameObject.Draw();
            }
        }

        /// <summary>
        /// Recieves packets from the server, updates objects or creates 
        /// objects based on the packet type
        /// </summary>
        private void ReceivePackets()
        {
            // Receive the response from the remote device.  
            networkClient.Receive();

            foreach (Packet packet in networkClient.PacketQueue)
            {
                if (packet is CreateObjectPacket)
                {
                    CreateObjectFromPacket(packet as CreateObjectPacket);
                }
                else
                {
                    gameObjects.TryGetValue(
                        packet.ObjectID, out GameObjectClient toUpdate);
                    toUpdate.UpdateFromPacket(packet);
                }
            }

            networkClient.PacketQueue.Clear();
        }

        /// <summary>
        /// Creates a new object from a given packet, whether that be a leaf 
        /// or a player.
        /// </summary>
        /// <param name="createPacket"></param>
        private void CreateObjectFromPacket(CreateObjectPacket createPacket)
        {
            switch (createPacket.objectType)
            {
                case (ObjectType.ACTIVE_PLAYER):
                    InitializeUserPlayerAndMovement(createPacket);
                    break;
                case (ObjectType.PLAYER):
                    gameObjects.Add(
                        createPacket.Id, new PlayerClient(createPacket)
                        );
                    break;
                case (ObjectType.LEAF):
                    break;
            }
        }

        /// <summary>
        /// Creates the activeplayer object and hooks up the input so that the 
        /// player moves
        /// </summary>
        /// <param name="createPacket">The createPacket that holds info 
        /// on intitial pos, etc</param>
        private void InitializeUserPlayerAndMovement(
            CreateObjectPacket createPacket
            )
        {
            ActivePlayer = new PlayerClient(createPacket);
            gameObjects.Add(ActivePlayer.Id, ActivePlayer);
            // Set up the input manager.
            SetupInputManager();
            GraphicsRenderer.Form.KeyDown +=
                InputManager.OnKeyDown;
            GraphicsRenderer.Form.KeyUp += InputManager.OnKeyUp;
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
