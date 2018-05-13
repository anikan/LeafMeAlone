using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Shared;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using System.Net;

namespace Client
{
    /// <summary>
    /// Client class that initializes the client adn runs the care game loop.
    /// </summary>
    class GameClient
    {

        // Offset of the camera from the player at origin.
        public static Vector3 CAMERA_OFFSET = new Vector3(0, 50, -30);

        // Active player on this client (who the user is playing as).
        private PlayerClient ActivePlayer;

        // Input manager that's receiving input and converting to actions.
        private InputManager InputManager;

        // Dictionary of all game objects in the game.
        private Dictionary<int, NetworkedGameObjectClient> NetworkedGameObjects;
        private List<NonNetworkedGameObjectClient> NonNetworkedGameObjects;

        // All leaves in the scene. 
        public List<LeafClient> leaves;

        // The active camera in the scene.
        private Camera Camera => GraphicsManager.ActiveCamera;

        // Timer to calculate time between frames.
        public Stopwatch FrameTimer;
        private UIFramesPersecond fps;
        private UITimer gameTimer;

        private NetworkClient networkClient;

        private static void Main(String[] args)
        {
            //Process.Start("..\\..\\..\\LeafMeAloneServer\\bin\\Debug\\LeafMeAloneServer.exe");

            IPAddress ipAddress = IPAddress.Loopback;
            if (args.Length > 0)
            { 
                IPHostEntry ipHostInfo = Dns.GetHostEntry(args[0]);
                ipAddress = ipHostInfo.AddressList[0];
            }

            // Create a new camera with a specified offset.
            Camera activeCamera = new Camera(CAMERA_OFFSET, Vector3.Zero, Vector3.UnitY);

            // Initialize graphics classes
            GraphicsRenderer.Init();
            GraphicsManager.Init(activeCamera);

            GameClient Client = new GameClient(new NetworkClient(ipAddress));


            //TODO FOR TESTING ONLY
            //GraphicsRenderer.Form.KeyDown += TestPlayerMovementWithoutNetworking;
            Client.fps = new UIFramesPersecond(new Size(5, 30), new Point(GraphicsRenderer.Form.ClientSize.Width - 30, 0));
            Client.gameTimer = new UITimer(60,new Size(225,3),new Point(0,0) );


            MessagePump.Run(GraphicsRenderer.Form, Client.DoGameLoop);

            GraphicsRenderer.Dispose();
        }

        
        private void DoGameLoop()
        {
            fps.Start();
            GraphicsRenderer.DeviceContext.ClearRenderTargetView(GraphicsRenderer.RenderTarget, new Color4(0.5f, 0.5f, 1.0f));
            GraphicsRenderer.DeviceContext.ClearDepthStencilView(GraphicsRenderer.DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);

            // Receive any packets from the server.
            ReceivePackets();

            // If there's an active player right now.
            if (ActivePlayer != null)
            {
                // Update input events.
                InputManager.Update();

                // Send any packets to the server.
                SendPackets();
            }

            // Update all objects.
            Update();

            // Draw everythhing.
            Render();


            GraphicsRenderer.BarContext.Draw();
            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);
            fps.StopAndCalculateFps();
        }

        // Start the networked client (connect to server).
        public GameClient(NetworkClient networkClient)
        {
            this.networkClient = networkClient;
            networkClient.StartClient();

            // Initialize frame timer
            FrameTimer = new Stopwatch();
            FrameTimer.Start();

            // Initialize game object lists.
            NetworkedGameObjects = new Dictionary<int, NetworkedGameObjectClient>();
            NonNetworkedGameObjects = new List<NonNetworkedGameObjectClient>();

            // TEMPORARY: Add the particle system to non-networked game objects.
            //NonNetworkedGameObjects.Add(p);

            // Receive the response from the remote device.  
            //networkClient.Receive();
        }

        /// <summary>
        /// Updates each of the game object's models with the time
        /// </summary>
        private void Update()
        {

            // Get the time since the last frame.
            float delta = FrameTimer.ElapsedMilliseconds;
            delta /= 1000.0f;

            // Iterate through all networked objects and update them.
            foreach (KeyValuePair<int, NetworkedGameObjectClient> kv in NetworkedGameObjects.AsEnumerable())
            {
                NetworkedGameObjectClient gameObject = kv.Value;

                // Update with delta in seconds
                gameObject.Update(delta);
            }

            // Iterate through all nonnetworked gameobjects and update them.
            foreach (GameObject obj in NonNetworkedGameObjects)
            {
                obj.Update(delta);
            }

            // Update the graphics manager.
            GraphicsManager.Update(delta);

            // Restart the frame timer.
            FrameTimer.Restart();

        }

        /// <summary>
        /// Loops through the hashtable of gameobjects and draws them
        /// </summary>
        private void Render()
        {

            // Iterate through all networked game objects and draw them.
            foreach (KeyValuePair<int, NetworkedGameObjectClient> kv in NetworkedGameObjects.AsEnumerable())
            {
                NetworkedGameObjectClient gameObject = kv.Value;
                gameObject.Draw();
            }

            // Iterate through all the non-networked objects and draw them.
            foreach (NonNetworkedGameObjectClient obj in NonNetworkedGameObjects)
            {
                obj.Draw();
            }
            GraphicsManager.Draw();
        }


        /// <summary>
        /// Recieves packets from the server, updates objects or creates 
        /// objects based on the packet type
        /// </summary>
        private void ReceivePackets()
        {
            // Receive the response from the remote device.  
            networkClient.Receive();

            // Iterate through every packet received.
            foreach (Packet packet in networkClient.PacketQueue)
            {
                // If this is a packet to create a new object, create the object.
                if (packet == null)
                {
                    continue;
                }

                if (packet is CreateObjectPacket)
                {
                    // Create the new object.
                    CreateObjectFromPacket(packet as CreateObjectPacket);
                }

                // Otherwise, if this is not a create packet.
                else
                {
                    // Get the object ID so we can update the object.
                    NetworkedGameObjects.TryGetValue(
                        packet.ObjectId, out NetworkedGameObjectClient toUpdate);

                    // If we didn't get an object, object doesn't exist and something is wrong.
                    if (toUpdate == null)
                    {
                        Console.WriteLine("Warning: Packet references object whose ID not created yet");
                    }

                    // Update the packet we found.
                    toUpdate.UpdateFromPacket(packet);
                }
            }

            // Clear the queue of packets.
            networkClient.PacketQueue.Clear();
        }

        /// <summary>
        /// Creates a new object from a given packet, whether that be a leaf 
        /// or a player.
        /// </summary>
        /// <param name="createPacket"></param>
        private void CreateObjectFromPacket(CreateObjectPacket createPacket)
        {

            // Create a new packet depending on it's type.
            switch (createPacket.objectType)
            {
                // Create an active player
                case (ObjectType.ACTIVE_PLAYER):
                    InitializeUserPlayerAndMovement(createPacket);
                    break;

                // Create an other player
                case (ObjectType.PLAYER):
                    NetworkedGameObjects.Add(
                        createPacket.ObjectId, new PlayerClient(createPacket)
                        );
                    break;

                // Create a leaf.
                case (ObjectType.LEAF):
                    NetworkedGameObjects.Add(
                        createPacket.ObjectId, new LeafClient(createPacket)

                        );
                    break;
            }
        }

        /// <summary>
        /// Creates the activeplayer object and hooks up the input so that the 
        /// player moves
        /// </summary>
        /// <param name="createPacket">The createPacket that holds info 
        /// on intitial pos, etc</param>
        private void InitializeUserPlayerAndMovement(CreateObjectPacket createPacket)
        {
            // Create a new player with the specified packet info.
            ActivePlayer = new PlayerClient(createPacket);

            // Set the active plyer in the graphics manager.
            GraphicsManager.ActivePlayer = ActivePlayer;

            // Add the player to networked game objects.
            NetworkedGameObjects.Add(ActivePlayer.Id, ActivePlayer);

            // Set up the input manager.
            SetupInputManager(ActivePlayer);
        }

        /// <summary>
        /// Sends out the data associated with the active player's input, resets requested movement
        /// </summary>
        private void SendPackets()
        {
            // Create a new player packet, and fill it with player's relevant info.
            PlayerPacket toSend = ClientPacketFactory.CreatePacket(ActivePlayer);
            byte[] data = toSend.Serialize();
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
        private void SetupInputManager(PlayerClient activePlayer)
        {
            // Create an input manager for player events.
            InputManager = new InputManager(activePlayer);

            // Input events for the input manager.
            GraphicsRenderer.Form.KeyDown += InputManager.OnKeyDown;
            GraphicsRenderer.Form.KeyUp += InputManager.OnKeyUp;
            GraphicsRenderer.Form.MouseDown += InputManager.OnMouseDown;
            GraphicsRenderer.Form.MouseUp += InputManager.OnMouseUp;
            GraphicsRenderer.Form.MouseMove += InputManager.OnMouseMove;
        }

    }
}
