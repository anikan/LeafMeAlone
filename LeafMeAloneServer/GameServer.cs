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
    class GameServer
    {
        private PlayerServer playerServer = new PlayerServer();

        private NetworkServer networkServer = new NetworkServer();

        //Time in ms for each tick.
        public const long TICK_TIME= 33;

        //Time per second for each tick.
        public const float TICK_TIME_S = .033f;

        private Stopwatch timer;

        public GameServer()
        {
            timer = new Stopwatch();
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
                    playerServer.UpdateFromPacket(networkServer.PlayerPackets[i]);
                }

                //Console.WriteLine("Player is at {0}", playerServer.GetTransform().Position);

                //Clear list for next frame.
                networkServer.PlayerPackets.Clear();

                //Send player data to all clients.
                networkServer.SendPlayer(playerServer);

                if ((int)(TICK_TIME - timer.ElapsedMilliseconds) < 0)
                {
                    Console.WriteLine("Warning: Server is falling behind.");
                }

                //Sleep for the rest of this tick.
                System.Threading.Thread.Sleep(Math.Max(0, (int)(TICK_TIME - timer.ElapsedMilliseconds)));
            }
        }
    }
}
