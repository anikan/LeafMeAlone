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

        public List<PlayerServer> playerServerList = new List<PlayerServer>();

        public List<LeafServer> LeafList = new List<LeafServer>();
        public Dictionary<int, GameObjectServer> gameObjectDict = new Dictionary<int, GameObjectServer>();

        private NetworkServer networkServer;

        //Time in ms for each tick.
        public const long TICK_TIME= 33;

        //Time per second for each tick.
        public const float TICK_TIME_S = .033f;

        private Stopwatch timer;

        private List<Vector3> spawnPoints = new List<Vector3>();

        private Stopwatch testTimer;

        public GameServer(IPAddress address)
        {
            if (instance != null)
            {
                Console.WriteLine("ERROR: Singleton pattern violated on GameServer.cs. There are multiple instances!");
            }

            instance = this; 

            timer = new Stopwatch();
            testTimer = new Stopwatch();

            timer.Start();
            testTimer.Start();

            spawnPoints.Add(new Vector3(-10, -10, 0));
            spawnPoints.Add(new Vector3(-10, 10, 0));
            spawnPoints.Add(new Vector3(10, -10, 0));
            spawnPoints.Add(new Vector3(10, 10, 0));

            networkServer = new NetworkServer(address);

            CreateRandomLeaves(200, -10, 10, -10, 10);

            //CreateLeaves(100, -10, 10, -10, 10);
        }

        public static int Main(String[] args)
        {
            IPAddress address;
            if (args.Length > 1)
            {
                address = IPAddress.Parse(args[1]);
            }

            else
            {
                address = IPAddress.Loopback;
            }

            GameServer gameServer = new GameServer(address);
            
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

                //Update the server players based on received packets.
                for (int i = 0; i < networkServer.PlayerPackets.Count(); i++)
                {
                    PlayerPacket packet = networkServer.PlayerPackets[i];

                    if (packet != null && gameObjectDict.TryGetValue(packet._ProtoObjId, out GameObjectServer playerGameObject))
                    {
                        PlayerServer player = (PlayerServer) playerGameObject;

                        player.UpdateFromPacket(networkServer.PlayerPackets[i]);

                   //     Console.WriteLine("Player {0} is at {1}", player.Id, player.Transform.Position);
                    }
                }

                //Clear list for next frame.
                networkServer.PlayerPackets.Clear();

                UpdateObjects(timer.ElapsedMilliseconds / 1000.0f);

                //Send object data to all clients.
                networkServer.SendWorldUpdateToAllClients();

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
            
            //This foreach loop hurts my soul. May consider making it normal for loop.
            foreach (KeyValuePair<int, GameObjectServer> pair in gameObjectDict )
            {
                pair.Value.Update(deltaTime);
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

            //Note currently assuming players get ids 0-3
            newActivePlayer.Transform.Position = spawnPoints[0];
            newPlayer.Transform.Position = spawnPoints[0];
            
            CreateObjectPacket objPacket = 
                new CreateObjectPacket(newPlayer);

            // Sending this new packet before the new client joins. 
             networkServer.SendAll(objPacket.Serialize());
               
            return newActivePlayer;
        }

        /// <summary>
        /// Creates all leaves in the scene, placing them randomly.
        /// </summary>
        /// <param name="num">Number of leaves to create.</param>
        /// <param name="minX">Min x position to spawn leaves.</param>
        /// <param name="maxX">Max x position to spawn leaves.</param>
        /// <param name="minY">Min y position to spawn leaves.</param>
        /// <param name="maxY">Max y position to spawn leaves.</param>
        public void CreateRandomLeaves(int num, float minX, float maxX, float minY, float maxY)
        {
            // Create a new random number generator.
            Random rnd = new Random();

            // Itereate through number of leaves we want to create.
            for (int i = 0; i < num; i++)
            {

                // Get random doubles for position.
                double randX = rnd.NextDouble();
                double randY = rnd.NextDouble();

                // Bind random doubles to our range.
                randX = (randX * (maxX - minX)) + minX;
                randY = (randY * (maxY - minY)) + maxY;

                // Get the new position
                Vector3 pos = new Vector3((float)randX, 0.0f, (float)randY);

                // Create a new leaf
                LeafServer newLeaf = new LeafServer();

                // Set the leaf's initial position.
                newLeaf.Transform.Position = pos;

                // Send this object to the other object's.
                networkServer.SendNewObjectToAll(newLeaf);

                // Add this leaf to the leaf list and object dictionary.
                LeafList.Add(newLeaf);
                newLeaf.Register();
            }
        }

        public void AddUniversalPhysics()
        {

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
    }
}
