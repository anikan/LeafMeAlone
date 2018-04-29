﻿using System;
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

        public List<GameObject> gameObjectList = new List<GameObject>();

        private NetworkServer networkServer = new NetworkServer();

        //Time in ms for each tick.
        public const long TICK_TIME= 33;

        //Time per second for each tick.
        public const float TICK_TIME_S = .033f;

        private Stopwatch timer;

        private List<Vector3> spawnPoints = new List<Vector3>();

        public GameServer()
        {
            instance = this; 

            timer = new Stopwatch();

            spawnPoints.Add(new Vector3(-10, -10, 0));
            spawnPoints.Add(new Vector3(-10, 10, 0));
            spawnPoints.Add(new Vector3(10, -10, 0));
            spawnPoints.Add(new Vector3(10, 10, 0));
            
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
                timer.Restart();

                //Check if a client wants to connect.
                networkServer.CheckForConnections();

                //Update the server players based on received packets.
                for (int i = 0; i < networkServer.PlayerPackets.Count(); i++)
                {


                    //playerServerList.UpdateFromPacket(networkServer.PlayerPackets[i]);
                }

                //Console.WriteLine("Player is at {0}", playerServer.GetTransform().Position);

                //Clear list for next frame.
                networkServer.PlayerPackets.Clear();

                //Send player data to all clients.
                //networkServer.SendPlayer(playerServer);

                if ((int)(TICK_TIME - timer.ElapsedMilliseconds) < 0)
                {
                    Console.WriteLine("Warning: Server is falling behind.");
                }

                //Sleep for the rest of this tick.
                System.Threading.Thread.Sleep(Math.Max(0, (int)(TICK_TIME - timer.ElapsedMilliseconds)));
            }
        }


        public PlayerServer CreateNewPlayer()
        {
            //Assign id based on the next spot in the gameObjectList.
            int id = gameObjectList.Count();

            PlayerServer newActivePlayer = new PlayerServer();
            newActivePlayer.objectType = ObjectType.ACTIVE_PLAYER;
            PlayerServer newPlayer = new PlayerServer();
            newPlayer.objectType = ObjectType.PLAYER;
            
            playerServerList.Add(newPlayer);
            gameObjectList.Add(newPlayer);

            //Note currently assuming players get ids 0-3
            newActivePlayer.Transform.Position = spawnPoints[id];
            newPlayer.Transform.Position = spawnPoints[id];
            
            CreateObjectPacket objPacket = 
                new CreateObjectPacket(newPlayer);

            //Sending this new packet before the new client joins. 
            networkServer.SendAll(objPacket);
               
            return newActivePlayer;
        }
    }
}
