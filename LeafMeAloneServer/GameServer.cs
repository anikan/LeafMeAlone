using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                System.Threading.Thread.Sleep(10);

            }
        }
    }
}
