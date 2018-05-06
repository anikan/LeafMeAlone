using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public List<GameObjectServer> gameObjectList = new List<GameObjectServer>();
        public List<LeafServer> LeafList = new List<LeafServer>();

        private NetworkServer networkServer = new NetworkServer();

        //Time in ms for each tick.
        public const long TICK_TIME= 33;

        //Time per second for each tick.
        public const float TICK_TIME_S = .033f;

        private Stopwatch timer;

        private List<Vector3> spawnPoints = new List<Vector3>();

        private Stopwatch testTimer;

        public GameServer()
        {
            instance = this; 

            timer = new Stopwatch();
            testTimer = new Stopwatch();

            timer.Start();
            testTimer.Start();

            spawnPoints.Add(new Vector3(-10, -10, 0));
            spawnPoints.Add(new Vector3(-10, 10, 0));
            spawnPoints.Add(new Vector3(10, -10, 0));
            spawnPoints.Add(new Vector3(10, 10, 0));

            CreateLeaves(1, -10, 10, -10, 10);
        }

        public static int Main(String[] args)
        {
            GameServer gameServer = new GameServer();
            
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
                    // playerServerList.UpdateFromPacket( networkServer.PlayerPackets[i] );
                }

                UpdateObjects(timer.ElapsedMilliseconds / 1000.0f);

                //Console.WriteLine("Player is at {0}", playerServer.GetTransform().Position);

                //Clear list for next frame.
                networkServer.PlayerPackets.Clear();

                //Send player data to all clients.
                //networkServer.SendPlayer(playerServer);

                if ((int)(TICK_TIME - timer.ElapsedMilliseconds) < 0)
                {
               //     Console.WriteLine("Warning: Server is falling behind.");
                }

                timer.Restart();

                //Sleep for the rest of this tick.
                System.Threading.Thread.Sleep(Math.Max(0, (int)(TICK_TIME - timer.ElapsedMilliseconds)));


            }
        }

        public void UpdateObjects(float deltaTime)
        {

            TestPhysics();


            for (int i = 0; i < gameObjectList.Count; i++)
            {
                gameObjectList[i].Update(deltaTime);
            }
        }

        public void TestPhysics()
        {

            if (testTimer.Elapsed.Seconds > 3)
            {

                for (int i = 0; i < LeafList.Count; i++)
                {
                    Console.WriteLine("APPLYING FORCE");
                    Vector3 testForce = new Vector3(100.0f, 0.0f, 0.0f);
                    LeafList[i].ApplyForce(testForce);
                    testTimer.Restart();

                }
            }

            for (int i = 0; i < LeafList.Count; i++)
            {
                string printString = string.Format("Leaf {0}: {1}", i, LeafList[i].Transform.Position);
                Console.WriteLine(printString);

            }
        }

        public PlayerServer CreateNewPlayer()
        {
            //Assign id based on the next spot in the gameObjectList.
            int id = gameObjectList.Count();

            PlayerServer newActivePlayer = new PlayerServer();
            newActivePlayer.ObjectType = ObjectType.ACTIVE_PLAYER;
            PlayerServer newPlayer = new PlayerServer();
            
            playerServerList.Add(newPlayer);
            gameObjectList.Add(newPlayer);

            //Note currently assuming players get ids 0-3
            newActivePlayer.Transform.Position = spawnPoints[0];
            newPlayer.Transform.Position = spawnPoints[0];
            
            CreateObjectPacket objPacket = 
                new CreateObjectPacket(newPlayer);

            // Sending this new packet before the new client joins. 
             networkServer.SendAll(objPacket.Serialize());
               
            return newActivePlayer;
        }

        public void CreateLeaves(int num, float minX, float maxX, float minY, float maxY)
        {
            Random rnd = new Random();

            for (int i = 0; i < num; i++)
            {

                double randX = rnd.NextDouble();
                double randY = rnd.NextDouble();

                randX = (randX * (maxX - minX)) + minX;
                randY = (randY * (maxY - minY)) + maxY;

                Vector3 pos = new Vector3((float)randX, 0.0f, (float)randY);

                LeafServer newLeaf = new LeafServer();
                newLeaf.Transform.Position = pos;
                gameObjectList.Add(newLeaf);
                LeafList.Add(newLeaf);

                Console.WriteLine("Creating leaf at position " + pos);
            }
        }

        public void AddUniversalPhysics()
        {

        }

        public void AddPlayerToolEffects()
        {

            for (int i = 0; i < playerServerList.Count; i++)
            {

                PlayerServer player = playerServerList[i];

                player.AffectObjectsInToolRange(gameObjectList);

            }
        }
    }
}
