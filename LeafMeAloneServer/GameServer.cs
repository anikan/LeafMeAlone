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

        public const long TICK_TIME= 33;

        //Time per second for each tick.
        public const float TICK_TIME_S = .033f;

        private float currentTime;

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

        public void DoGameLoop()
        {
            while (true)
            {
                timer.Restart();

                networkServer.CheckForConnections();

                for (int i = 0; i < networkServer.PlayerPackets.Count(); i++)
                {
                    playerServer.UpdateFromPacket(networkServer.PlayerPackets[i]);
                }

                Console.WriteLine("Player is at {0}", playerServer.GetTransform().Position);

                //Clear for next frame.
                networkServer.PlayerPackets.Clear();

                networkServer.SendPlayer(playerServer);

                Console.WriteLine("Sleeping for {0}", (int)(TICK_TIME - timer.ElapsedMilliseconds));
                
                System.Threading.Thread.Sleep(Math.Max(0, (int)(TICK_TIME - timer.ElapsedMilliseconds)));
            }
        }
    }
}
