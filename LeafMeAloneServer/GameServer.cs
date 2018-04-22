using System;
using System.Collections.Generic;
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
                networkServer.CheckForConnections();

                for (int i = 0; i < networkServer.PlayerPackets.Count(); i++)
                {
                    playerServer.UpdateFromPacket(networkServer.PlayerPackets[i]);
                }

                //Clear for next frame.
                networkServer.PlayerPackets.Clear();

                networkServer.SendPlayer(playerServer);
                
                System.Threading.Thread.Sleep(10);
            }
        }

    }
}
