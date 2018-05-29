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
using Shared.Packet;
using SlimDX.DirectWrite;
using SpriteTextRenderer;
using TextBlockRenderer = SpriteTextRenderer.SlimDX.TextBlockRenderer;

namespace Client
{
    /// <summary>
    /// Client class that initializes the client adn runs the care game loop.
    /// </summary>
    class GameClient
    {
        private const string BAD_PACKET_REF =
            "Warning: Packet references object whose ID not created yet";

        // Offset of the camera from the player at origin.
        public static Vector3 CAMERA_OFFSET = new Vector3(0, 50, -30);

        // Active player on this client (who the user is playing as).
        private PlayerClient ActivePlayer;

        // Input manager that's receiving input and converting to actions.
        private InputManager InputManager;

        // Dictionary of all game objects in the game.
        private Dictionary<int, NetworkedGameObjectClient> NetworkedGameObjects;

        private List<NonNetworkedGameObjectClient> NonNetworkedGameObjects;

        private ClientPacketHandler clientPacketHandler;

        // All leaves in the scene. 
        public List<LeafClient> leaves;

        internal PlayerClient GetActivePlayer()
        {
            return ActivePlayer;
        }

        internal void DoPlayerDeath()
        {
            Console.WriteLine("Player Died");
        }

        /// <summary>
        /// The ID of the audio pool to be used by all the leaves collectively
        /// </summary>
        public int leafAudioPoolId;
        public const int LeafAudioCapacity = 50;

        // The active camera in the scene.
        private Camera Camera => GraphicsManager.ActiveCamera;

        // Timer to calculate time between frames.
        public Stopwatch FrameTimer;
        private UIFramesPersecond fps;
        private UITimer gameTimer;
        

        private NetworkClient networkClient;

        public static GameClient instance;

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
            Camera activeCamera =
                new Camera(CAMERA_OFFSET, Vector3.Zero, Vector3.UnitY);

            // Initialize graphics classes
            GraphicsRenderer.Init();
            GraphicsManager.Init(activeCamera);
            AudioManager.Init();

            GameClient Client = new GameClient(new NetworkClient(ipAddress));


            //TODO FOR TESTING ONLY
            //GraphicsRenderer.Form.KeyDown += 
            // TestPlayerMovementWithoutNetworking;
            Client.fps = new UIFramesPersecond(new Size(5, 30),
                new Point(GraphicsRenderer.Form.ClientSize.Width - 30, 0));
            Client.gameTimer =
                new UITimer(60, new Size(225, 3), new Point(0, 0));
            
            MessagePump.Run(GraphicsRenderer.Form, Client.DoGameLoop);

            GraphicsRenderer.Dispose();

        }

        internal Team GetPlayerTeam()
        {
            return ActivePlayer.team;
        }
        
        private void DoGameLoop()
        {
            fps.Start();
            GraphicsRenderer.DeviceContext.ClearRenderTargetView(
                GraphicsRenderer.RenderTarget, new Color4(0.0f, .4f, 0.0f));
            GraphicsRenderer.DeviceContext.ClearDepthStencilView(
                GraphicsRenderer.DepthView, DepthStencilClearFlags.Depth,
                1.0f, 0);
            
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


            UIManager2.DrawText("Hello", UIManager2.TextType.COMIC_SANS, new Vector2(100, 100), Color.Pink);
            UIManager2.DrawText("Hello", UIManager2.TextType.NORMAL, new Vector2(100, 120), Color.Green);
            UIManager2.DrawText("Hello", UIManager2.TextType.BOLD, new Vector2(100, 140), Color.Red);

            // Update all objects.
            Update();

            // Draw everythhing.
            Render();
            

            GraphicsRenderer.BarContext.Draw();
            UIManager2.Update();
            UIManager2.SpriteRenderer.Flush();
            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);
            fps.StopAndCalculateFps();

            AudioManager.Update();
        }

        // Start the networked client (connect to server).
        public GameClient(NetworkClient networkClient)
        {
            if (instance != null)
            {
                Console.WriteLine("WARNING: Attempting to double instantiate GameClient!");
            }
            instance = this;

            this.networkClient = networkClient;
            networkClient.StartClient();

            this.clientPacketHandler = new ClientPacketHandler(this);

            // Initialize frame timer
            FrameTimer = new Stopwatch();
            FrameTimer.Start();

            // Initialize game object lists.
            NetworkedGameObjects = new Dictionary<int, NetworkedGameObjectClient>();
            NonNetworkedGameObjects = new List<NonNetworkedGameObjectClient>();

            leafAudioPoolId = AudioManager.NewSourcePool(LeafAudioCapacity);

            // TEMPORARY: Add the particle system to non-networked game objects.
            //NonNetworkedGameObjects.Add(p);

            // Receive the response from the remote device.  
            //networkClient.Receive();
        }

        // Create a map on the client and add it to the objects.
        public void CreateMap()
        {
            MapClient gameMap = new MapClient();
            NonNetworkedGameObjects.Add(gameMap);
        }

        /// <summary>
        /// Updates each of the game object's models with the time
        /// </summary>
        private void Update()
        {

            // Get the time since the last frame.
            float delta = FrameTimer.ElapsedMilliseconds;
            delta /= 1000.0f;
            delta = Math.Max(.001f, delta);
            delta = Math.Min(.01f, delta);
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
            foreach (KeyValuePair<int, NetworkedGameObjectClient> kv in
                NetworkedGameObjects.AsEnumerable())
            {
                NetworkedGameObjectClient gameObject = kv.Value;
                gameObject.Draw();
            }

            // iterate through all the non-networked objects and draw them.
            foreach (
                NonNetworkedGameObjectClient obj in NonNetworkedGameObjects
                )
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
            foreach (BasePacket packet in networkClient.PacketQueue)
            {
                // If this is a packet to create a new object, create the object.
                if (packet == null)
                {
                    continue;
                }

                clientPacketHandler.HandlePacket(packet);
            }
            // Clear the queue of packets.
            networkClient.PacketQueue.Clear();
        }

        /// <summary>
        /// Creates a new object from a given packet, whether that be a leaf 
        /// or a player.
        /// </summary>
        /// <param name="createPacket"></param>
        /// <returns>The object which was created</returns>
        public GameObject CreateObjectFromPacket(CreateObjectPacket createPacket)
        {

            int objId = createPacket.ObjData.IdData.ObjectId;

            // Create a new packet depending on it's type.
            switch (createPacket.ObjectType)
            {
                // Create an active player
                case (ObjectType.ACTIVE_PLAYER):
                    return InitializeUserPlayerAndMovement(createPacket);
                // Create an other player
                case (ObjectType.PLAYER):
                    NetworkedGameObjectClient player = new PlayerClient(createPacket);
                    NetworkedGameObjects.Add(objId, player);
                    return player;
                // Create a leaf.
                case (ObjectType.LEAF):
                    NetworkedGameObjectClient leaf = new LeafClient(createPacket);
                    NetworkedGameObjects.Add(objId, leaf);
                    return leaf;
                case (ObjectType.TREE):
                    Transform startTransform = new Transform();
                    float initX = createPacket.ObjData.PositionX;
                    float initY = createPacket.ObjData.PositionY;
                    float initZ = createPacket.ObjData.PositionZ;
                    startTransform.Position = new Vector3(initX, initY, initZ);
                    NetworkedGameObjectClient tree = new TreeClient(createPacket);
                    NetworkedGameObjects.Add(objId, tree);
                    return tree;
            }
            return null;
        }

        /// <summary>
        /// Creates the activeplayer object and hooks up the input so that the 
        /// player moves
        /// </summary>
        /// <param name="createPacket">The createPacket that holds info 
        /// on intitial pos, etc</param>
        /// <returns>the created player</returns>
        private GameObject InitializeUserPlayerAndMovement(
            CreateObjectPacket createPacket
            )
        {
            // Create a new player with the specified packet info.
            ActivePlayer = new PlayerClient(createPacket);

            // Set the active plyer in the graphics manager.
            GraphicsManager.ActivePlayer = ActivePlayer;

            // Add the player to networked game objects.
            NetworkedGameObjects.Add(ActivePlayer.Id, ActivePlayer);

            // Set up the input manager.
            SetupInputManager(ActivePlayer);

            CreateMap();

            return ActivePlayer;
        }

        /// <summary>
        /// Sends out the data associated with the active player's input, 
        /// resets requested movement
        /// </summary>
        private void SendPackets()
        {
            // Create a new player packet, and fill it with player info.
            RequestPacket toSend =
                ClientPacketFactory.CreateRequestPacket(ActivePlayer);
            byte[] data = PacketUtil.Serialize(toSend);
            networkClient.Send(data);

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

        /// <summary>
        /// Destroys the game object on the client 
        /// </summary>
        /// <param name="gameObj">the game object to destroy</param>
        public void Destroy(GameObject gameObj)
        {
            if (gameObj is NetworkedGameObjectClient networkedObj)
            {
                NetworkedGameObjects.Remove(networkedObj.Id);
            }
            else if (gameObj is NonNetworkedGameObjectClient nonNetObj)
            {
                NonNetworkedGameObjects.Remove(nonNetObj);
            }
        }

        internal NetworkedGameObjectClient GetObjectFromPacket(IIdentifiable p)
        {
            int id = p.GetId();
            NetworkedGameObjects.TryGetValue(id, out NetworkedGameObjectClient packetObject);
            return packetObject;
        }
    }
}
