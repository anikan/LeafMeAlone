using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Server
{
    public class GameServer
    {
        public static GameServer instance;

        public List<GameObject> toDestroyQueue = new List<GameObject>();
        public List<PlayerServer> playerServerList = new List<PlayerServer>();
        public Dictionary<int, GameObjectServer> gameObjectDict =
            new Dictionary<int, GameObjectServer>();

        private NetworkServer networkServer;

        //Time in ms for each tick.
        public const long TICK_TIME = 33;

        //Time per second for each tick.
        public const float TICK_TIME_S = .033f;
        private const string SINGLETON_VIOLATED =
            "ERROR: Singleton pattern violated on GameServer.cs. There are multiple instances!";
        private Stopwatch timer;

        private List<Vector3> spawnPoints = new List<Vector3>();
        private int playerSpawnIndex = 0;

        private Stopwatch testTimer;

        public GameServer(bool networked)
        {
            if (instance != null)
            {
                Console.WriteLine(SINGLETON_VIOLATED);
            }

            instance = this;

            timer = new Stopwatch();
            testTimer = new Stopwatch();

            timer.Start();
            testTimer.Start();

            spawnPoints.Add(new Vector3(-10, 0, -10));
            spawnPoints.Add(new Vector3(-10, 0, 10));
            spawnPoints.Add(new Vector3(10, 0, -10));
            spawnPoints.Add(new Vector3(10, 0, 10));

            networkServer = new NetworkServer(networked);

            // Create the initial game map.
            CreateMap();

            // Variables for determining leaf spawn locations.
            float HalfWidth = Constants.MAP_WIDTH / 2.0f;
            float HalfHeight = Constants.MAP_HEIGHT / 2.0f;

            // Create the leaves for the game.
            CreateRandomLeaves(Constants.NUM_LEAVES);

            //CreateLeaves(100, -10, 10, -10, 10);
        }

        public static int Main(String[] args)
        {
            bool networked = false;

            if (args.Length > 0)
            {
                networked = true;
            }

            GameServer gameServer = new GameServer(networked);

            gameServer.networkServer.StartListening();

            gameServer.DoGameLoop();

            return 0;
        }

        /// <summary>
        /// Main game loop.
        /// </summary>
        public void DoGameLoop()
        {
            while (true)
            {

                //Check if a client wants to connect.
                networkServer.CheckForConnections();

                // Go ahead and try to receive new updates
                networkServer.Receive();

                //Update the server players based on received packets.
                for (int i = 0; i < networkServer.PlayerPackets.Count(); i++)
                {
                    PlayerPacket packet = networkServer.PlayerPackets[i];

                    if (packet != null &&
                        gameObjectDict.TryGetValue(packet._ProtoObjId, out GameObjectServer playerGameObject)
                        )
                    {
                        PlayerServer player = (PlayerServer)playerGameObject;

                        player.UpdateFromPacket(networkServer.PlayerPackets[i]);
                    }
                }

                //Clear list for next frame.
                networkServer.PlayerPackets.Clear();

                UpdateObjects(timer.ElapsedMilliseconds / 1000.0f);

                //Send object data to all clients.
                networkServer.SendWorldUpdateToAllClients();
                toDestroyQueue.Clear();

                if ((int)(TICK_TIME - timer.ElapsedMilliseconds) < 0)
                {
                    //     Console.WriteLine("Warning: Server is falling behind.");
                }

                timer.Restart();

                //Sleep for the rest of this tick.
                System.Threading.Thread.Sleep(Math.Max(0, (int)(TICK_TIME - timer.ElapsedMilliseconds)));
            }
        }

        /// <summary>
        /// Call Update() on all objects in the object dict.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateObjects(float deltaTime)
        {
            //TestPhysics();

            List<GameObjectServer> toUpdateList = gameObjectDict.Values.ToList();
            //This foreach loop hurts my soul. May consider making it normal for loop.
            foreach (GameObjectServer toUpdate in toUpdateList)
            {
                toUpdate.Update(deltaTime);
            }

            // Add the effects of the player tools.
            AddPlayerToolEffects();

        }

        public PlayerServer CreateNewPlayer()
        {
            //Assign id based on the next spot in the gameObjectDict.
            int id = gameObjectDict.Count();

            //Create two players, one to send as an active player to client. Other to keep track of on server.
            PlayerServer newPlayer = new PlayerServer();
            newPlayer.Register();

            //Create the active player with the same id as the newPlayer.
            PlayerServer newActivePlayer = new PlayerServer();
            newActivePlayer.ObjectType = ObjectType.ACTIVE_PLAYER;
            newActivePlayer.Id = newPlayer.Id;

            playerServerList.Add(newPlayer);

            Vector3 nextSpawnPoint = spawnPoints[(playerSpawnIndex++ % spawnPoints.Count)];

            //Note currently assuming players get ids 0-3
            newActivePlayer.Transform.Position = nextSpawnPoint;
            newPlayer.Transform.Position = nextSpawnPoint;

            CreateObjectPacket objPacket =
                new CreateObjectPacket(newPlayer);

            // Sending this new packet before the new client joins. 
            networkServer.SendAll(objPacket.Serialize());

            return newActivePlayer;
        }

        /// <summary>
        /// Creates the initial game map.
        /// </summary>
        /// <returns>The new map</returns>
        public MapServer CreateMap()
        {

            // Create the map with a width and height.
            MapServer newMap = new MapServer(Constants.MAP_WIDTH, Constants.MAP_HEIGHT);

            // Spawn trees around the border of the map!
            // Start by iterating through the height of the map, centered on origin and increase by the radius of a tree.
            for (float y = -newMap.Height / 2.0f; y < newMap.Height / 2.0f; y+= TreeServer.TREE_RADIUS)
            {

                // Iterate through the width of the map, centered on origin and increase by radius of a tree.
                for (float x = -newMap.Width / 2.0f; x < newMap.Width / 2.0f; x+= TreeServer.TREE_RADIUS)
                {

                    // If this is a top or bottom row, create trees.
                    if (y <= -newMap.Height / 2.0f || (newMap.Height / 2.0f) <= y + TreeServer.TREE_RADIUS)
                    {

                        // Make a new tree.
                        TreeServer newTree = new TreeServer();

                        // Set the tree's initial position.
                        newTree.Transform.Position = new Vector3(x, Constants.FLOOR_HEIGHT, y);

                        // Send the new object to client.
                        networkServer.SendNewObjectToAll(newTree);

                    }

                    // If this is the far left or right columns, create a tree.
                    else if (x <= -newMap.Width / 2.0f || (newMap.Width / 2.0f) <= x + TreeServer.TREE_RADIUS)
                    {

                        // Make a new tree.
                        TreeServer newTree = new TreeServer();

                        // Set the tree's initial position.
                        newTree.Transform.Position = new Vector3(x, Constants.FLOOR_HEIGHT, y);

                        // Send the new object to client.
                        networkServer.SendNewObjectToAll(newTree);

                    }
                }
            }

            // Return the new map.
            return newMap;

        }

        /// <summary>
        /// Creates all leaves in the scene, placing them randomly.
        /// </summary>
        /// <param name="num">Number of leaves to create.</param>
        public void CreateRandomLeaves(int num)
        {

            // Itereate through number of leaves we want to create.
            for (int i = 0; i < num; i++)
            {

                CreateRandomLeaf();
               
            }
        }

        public void CreateRandomLeaf()
        {
            // Create a new random number generator.
            Random rnd = new Random();

            // Very slight random offset for leaves so that there's no z-fighting.
            double minY = Constants.FLOOR_HEIGHT;
            double maxY = Constants.FLOOR_HEIGHT + 0.2f;

            // Variables for determining leaf spawn locations.
            float HalfWidth = Constants.MAP_WIDTH / 2.0f;
            float HalfHeight = Constants.MAP_HEIGHT / 2.0f;

            float minX = -HalfWidth + Constants.BORDER_MARGIN;
            float maxX = HalfWidth - Constants.BORDER_MARGIN;
            float minZ = -HalfHeight + Constants.BORDER_MARGIN;
            float maxZ = HalfHeight - Constants.BORDER_MARGIN;

            // Get random doubles for position.
            double randX = rnd.NextDouble();
            double randY = rnd.NextDouble();
            double randZ = rnd.NextDouble();

            // Bind random doubles to our range.
            randX = (randX * (maxX - minX)) + minX;
            randY = (randY * (maxY - minY)) + minY;
            randZ = (randZ * (maxZ - minZ)) + minZ;

            // Get the new position
            Vector3 pos = new Vector3((float)randX, (float)randY, (float)randZ);

            // Create a new leaf
            LeafServer newLeaf = new LeafServer();

            // Set the leaf's initial position.
            newLeaf.Transform.Position = pos;

            // Send this object to the other object's.
            networkServer.SendNewObjectToAll(newLeaf);

            // Add this leaf to the leaf list and object dictionary.
            newLeaf.Register();

        }

        /// <summary>
        /// Add the tool effects of all the players.
        /// </summary>
        public void AddPlayerToolEffects()
        {

            // Iterate through all players.
            for (int i = 0; i < playerServerList.Count; i++)
            {

                // Get this player.
                PlayerServer player = playerServerList[i];

                // Affect all objects within range of the player.
                player.AffectObjectsInToolRange(gameObjectDict.Values.ToList<GameObjectServer>());

            }
        }

        /// <summary>
        /// Destroys the given game object; removes it from the dictionary of 
        /// objects to send update packets for, and adds the destory packet to 
        /// the toDestroy Queue
        /// </summary>
        /// <param name="gameObj">The game object to destroy</param>
        public void Destroy(GameObject gameObj)
        {

            gameObjectDict.Remove(gameObj.Id);

            if (gameObj is PlayerServer player)
            {
                playerServerList.Remove(player);
            }

            if (gameObj is LeafServer)
            {
                CreateRandomLeaf();
            }

            toDestroyQueue.Add(gameObj);
        }

        /// <summary>
        /// Gets a list of game objects from the dictionary.
        /// </summary>
        /// <returns>A list of all objects from the object dictionary.</returns>
        public List<GameObjectServer> GetGameObjectList()
        {
            // Turn the game objects to a value list.
            return gameObjectDict.Values.ToList<GameObjectServer>();
        }
    }
}
