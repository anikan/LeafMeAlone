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
using System.Threading;
using System.Threading.Tasks;
using Client.UI;
using Shared.Packet;
using SlimDX.DirectWrite;
using SpriteTextRenderer;
using TextBlockRenderer = SpriteTextRenderer.SlimDX.TextBlockRenderer;
using System.IO;

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

        private bool HasInitted = false;
        bool hasConnected = false;

        // make win logic easier to handle
        // check if the current match is over and new match is not started
        public bool PendingRematchState = false;
        public TeamName WinningTeam = TeamName.BLUE;

        //Keeps track of when the client can send a packet.
        private Stopwatch sendTimer = new Stopwatch();

        //Amount of ms to wait before sending a new packet.
        private float sendDelay = 16.6f;


        // All leaves in the scene. 
        public List<LeafClient> leaves;

        public List<PlayerClient> playerClients = new List<PlayerClient>();

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
        public int AudioPoolLeafBurning, AudioPoolLeafMoving;
        public const int LeafAudioCapacity = 50;

        // The active camera in the scene.
        private Camera Camera => GraphicsManager.ActiveCamera;

        // Timer to calculate time between frames.
        public Stopwatch FrameTimer;
        private NetworkClient networkClient;

        private Match activeMatch = Match.DefaultMatch;

        private int _audioBGM;

        private RequestPacket lastRequest = null;

        public static GameClient instance;

        private float volume = Constants.DEFAULT_VOLUME;

        private static void Main(String[] args)
        {
            //Process.Start("..\\..\\..\\LeafMeAloneServer\\bin\\Debug\\LeafMeAloneServer.exe");

                // Initialize static classes
                GraphicsRenderer.Init();

                //catch (FormatException e)
                //{
                //    IPHostEntry ipHostInfo = Dns.GetHostEntry(args[0]);
                //    ipAddress = ipHostInfo.AddressList[0];
                //}
           // }

            // Create a new camera with a specified offset.
            Camera activeCamera =
                new Camera(CAMERA_OFFSET, Vector3.Zero, Vector3.UnitY);
            GraphicsManager.Init(activeCamera);
            AudioManager.Init();
            AudioManager.SetListenerVolume(8.0f);
            AnimationManager.Init();

            GameClient Client = new GameClient();
            GlobalUIManager.Init();
            
            GraphicsRenderer.connectButton.Click += (sender, eventArgs) =>
                {
                    if (!Client.hasConnected)
                    {
                        Client.hasConnected = true;
                        IPAddress ipAddress = IPAddress.Loopback;
                            if (GraphicsRenderer.networkedCheckbox.Checked)
                            {
                                ipAddress = IPAddress.Parse(GraphicsRenderer.ipTextbox.Text);
                                //var ipHostEntry = Dns.GetHostEntry(GraphicsRenderer.ipTextbox.Text);
                                //ipAddress = ipHostEntry.AddressList[0];
                                Console.WriteLine($" ip is {ipAddress.ToString()}");
                            }

                        Client.Init(new NetworkClient(ipAddress));
                        GraphicsRenderer.Panel1.Visible = false;
                        GraphicsRenderer.Panel1.Hide();
                        GraphicsRenderer.pictureBox1.Visible = false;
                        GraphicsRenderer.pictureBox1.Hide();
                        GraphicsRenderer.Form.Focus();
                    }
                };

            MessagePump.Run(GraphicsRenderer.Form, Client.DoGameLoop);

            GraphicsRenderer.Dispose();
        }

        internal void ResetGameTimer()
        {
            GlobalUIManager.gameTimer.End();
        }

        internal TeamName GetPlayerTeam()
        {
            return ActivePlayer.PlayerTeam;
        }
        
        private void DoGameLoop()
        {
            if (!HasInitted)
                return;

            GlobalUIManager.fps.Start();
            GraphicsRenderer.DeviceContext.ClearRenderTargetView(
                GraphicsRenderer.RenderTarget, new Color4(0.0f, .4f, 0.0f));
            GraphicsRenderer.DeviceContext.ClearDepthStencilView(
                GraphicsRenderer.DepthView, DepthStencilClearFlags.Depth,
                1.0f, 0);

            if (NetworkClient.PendingReset)
            {
                HasInitted = false;
                hasConnected = false;
                NetworkClient.PendingReset = false;
                GraphicsRenderer.Panel1.Visible = true;
                GraphicsRenderer.Panel1.Show();
                GraphicsRenderer.pictureBox1.Visible = true;
                GraphicsRenderer.pictureBox1.Show();
                GraphicsRenderer.Panel1.Focus();
                return;
            }


            // Receive any packets from the server.
            ReceivePackets();
            // If there's an active player right now.
            if (ActivePlayer != null)
            {
                // Update input events.
                InputManager.Update();

                //If enough time has passed since the last packet, send.
                if (sendTimer.ElapsedMilliseconds > sendDelay)
                {
                    // Send any packets to the server.
                    SendRequest();

                    sendTimer.Restart();
                }
            }


            // Update all objects.
            Update();

            // Draw everythhing.
            Render();
            
            
            GlobalUIManager.Update();
            UIManagerSpriteRenderer.Update();
            UIManagerSpriteRenderer.SpriteRenderer.Flush();
            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);
            GlobalUIManager.fps.StopAndCalculateFps();
            AudioManager.Update();

        }

        internal void StartMatchTimer(float gameTime)
        {
            GlobalUIManager.gameTimer.Start(gameTime);
        }

        public void Init(NetworkClient client)
        {
            this.networkClient = client;
            networkClient.StartClient();

            this.clientPacketHandler = new ClientPacketHandler(this);

            // Initialize frame ElapsedTime
            FrameTimer = new Stopwatch();
            FrameTimer.Start();

            // Initialize game object lists.
            NetworkedGameObjects = new Dictionary<int, NetworkedGameObjectClient>();
            NonNetworkedGameObjects = new List<NonNetworkedGameObjectClient>();

            AudioPoolLeafBurning = AudioManager.NewSourcePool(LeafAudioCapacity, 0.6f);
            AudioPoolLeafMoving = AudioManager.NewSourcePool(LeafAudioCapacity);

            _audioBGM = AudioManager.GetNewSource();
            AudioManager.PlayAudio(_audioBGM, Constants.Bgm, true);
            AudioManager.SetSourceVolume(_audioBGM, 0.05f);
            HasInitted = true;
        }

        // Start the networked client (connect to server).
        public GameClient()
        {
            if (instance != null)
            {
                Console.WriteLine("WARNING: Attempting to double instantiate GameClient!");
            }

            instance = this;

            AudioManager.SetListenerVolume(volume);

            // TEMPORARY: Add the particle system to non-networked game objects.
            //NonNetworkedGameObjects.Add(p);

            // Receive the response from the remote device.  
            //networkClient.Receive();
            sendTimer.Start();
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

            // Tint all of the leaves based on their sections.
            TintLeaves();
            CountLeaves();

            // Update the graphics manager.
            GraphicsManager.Update(delta);
            AudioManager.Update();
            AnimationManager.Update(delta);

            // Restart the frame ElapsedTime.
            FrameTimer.Restart();

            //AudioManager.UpdateSourceLocation(_audioBGM, Camera.CameraPosition);
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
                    var newPlayer = InitializeUserPlayerAndMovement(createPacket) as PlayerClient;
                    playerClients.Add(newPlayer);
                    newPlayer.Name = GraphicsRenderer.nicknameTextbox.Text;
                    return newPlayer;
                // Create an other player
                case (ObjectType.PLAYER):
                    NetworkedGameObjectClient player = new PlayerClient(createPacket);
                    playerClients.Add((PlayerClient) player);
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
        /// Sends out the data associated with the active player's input if the player is requesting something different, 
        /// resets requested movement.
        /// 
        /// </summary>
        private void SendRequest()
        {
            // Create a new player packet, and fill it with player info.
            RequestPacket toSend =
                ClientPacketFactory.CreateRequestPacket(ActivePlayer);

            //If this is sending different data from before, send it.
            if (!RequestPacket.equals(toSend, lastRequest))
            {
                byte[] data = PacketUtil.Serialize(toSend);
                networkClient.Send(data);

                lastRequest = toSend;
            }

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
        /// <summary>
        /// Tint all the leaves in the game.
        /// </summary>
        public void TintLeaves()
        {
            // Get a list of all leaves.
            List<LeafClient> leaves = GetLeafList();

            // Itereate through the leaves.
            foreach (LeafClient leaf in leaves)
            {
                // Iterate through all team sections.
                for (int index = 0; index < activeMatch.teams.Count; index++)
                {
                    TeamSection section = activeMatch.teams[index].teamSection;
                    // If this leaf is in this team section.
                    if (section.IsInBounds(leaf.Transform.Position))
                    {
                        // Tint the leaf to section.
                        leaf.CurrentHue = section.sectionColor;
                    }
                }
                // Check if this leaf is in no mans land.
                if (activeMatch.NoMansLand.IsInBounds(leaf.Transform.Position))
                {
                    // Tint the leaf.
                    leaf.CurrentHue = activeMatch.NoMansLand.sectionColor;
                }
            }
        }

        /// <summary>
        /// Count the number of leaves on each side and set the UI to the correct value.
        /// </summary>
        public void CountLeaves()
        {

            for (int index = 0; index < activeMatch.teams.Count; index++)
            {

                TeamSection section = activeMatch.teams[index].teamSection;

                int leafCount = activeMatch.GetTeamLeaves(index, GetLeafListAsGameObjects());

                if (index == 0)
                {
                    GlobalUIManager.Teams.Team1_Leaves.UIText.Text = leafCount.ToString();
                }
                else
                {
                    GlobalUIManager.Teams.Team2_Leaves.UIText.Text = leafCount.ToString();
                }
            }


        }

        /// <summary>
        /// Get a list of all leaves from the game object list.
        /// </summary>
        /// <returns>List of all leaves.</returns>
        public List<LeafClient> GetLeafList()
        {
            // List to return.
            List<LeafClient> allLeaves = new List<LeafClient>();

            // Get all object values from the game object dictionary.
            List<NetworkedGameObjectClient> allObjects = NetworkedGameObjects.Values.ToList<NetworkedGameObjectClient>();

            // Iterate through all objects.
            foreach (NetworkedGameObjectClient obj in allObjects)
            {

                // If it's a leaf.
                if (obj is LeafClient leaf)
                {
                    // Add it to the leaf list.
                    allLeaves.Add(leaf);
                }
            }

            // Return all the leaves.
            return allLeaves;
        }

        public List<GameObject> GetLeafListAsGameObjects()
        {

            GameObject[] leaves = GetLeafList().ToArray();
            return leaves.ToList<GameObject>();

        }

        /// <summary>
        /// Change the volume, either increase or decrease.
        /// </summary>
        /// <param name="sign">-1 or 1, depending on volume increase / decrease. </param>
        public void ChangeVolume(int sign)
        {

            // Clamp the sign to -1 or 1. Would be so much easier if Math.Clamp was a thing.
            if (sign < 0)
            {
                sign = -1;
            }
            else if (sign > 0)
            {
                sign = 1;
            }
            
            // Increase the volume.
            volume += (sign * Constants.VOLUME_INCREASE);

            // Set the volume.
            AudioManager.SetListenerVolume(volume);
        }


        /// <summary>
        /// Save player stats to file.
        /// </summary>
        /// <param name="st">Player stats</param>
        public void SaveStats(PlayerStats st)
        {
            // Make the Stats directory if it doesn't exist.
            if (!Directory.Exists(Constants.STATS_DIRECTORY))
            {
                Directory.CreateDirectory(Constants.STATS_DIRECTORY);
            }

            // The current date time, with format string.
            string dateTime = DateTime.Now.ToString("hh-mm-ss_dd_MM_yyyy");

            // String for the file to save.
            string fileString = Constants.STATS_PREFIX + dateTime + ".txt";

            // Full path to save.
            string fullPath = Constants.STATS_DIRECTORY + fileString;

            // Write all the stats to the designated file.
            File.WriteAllText(fullPath, st.ToString());

        }
    }
}
