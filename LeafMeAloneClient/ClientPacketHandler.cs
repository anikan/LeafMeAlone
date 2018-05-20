using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class ClientPacketHandler
    {
        private Dictionary<PacketType, Action<Packet>> packetHandlers;
        private GameClient client;

        public ClientPacketHandler(GameClient client)
        {
            this.client = client;

            void UpdateAction(Packet p)
            {
                NetworkedGameObjectClient packetObject = client.GetObjectFromPacket((IIdentifiable)p);
                packetObject.UpdateFromPacket(p);
            }

            void DestroyAction(Packet p)
            {
                NetworkedGameObjectClient packetObject = client.GetObjectFromPacket((IIdentifiable)p);
                packetObject.Destroy();
            }
            
            void CreateObjectAction(Packet p)
            {
                client.CreateObjectFromPacket(((CreateObjectPacket)p));
            }

            packetHandlers = new Dictionary<PacketType, Action<Packet>>()
                {
                    {PacketType.CreatePlayerPacket, p => {
                        CreateObjectAction(((CreatePlayerPacket)p).createPacket);
                    } },
                    {PacketType.CreateObjectPacket, CreateObjectAction },
                    {PacketType.ObjectPacket, UpdateAction },
                    {PacketType.PlayerPacket, UpdateAction },
                    {PacketType.DestroyObjectPacket, DestroyAction},
                };
        }

        internal void HandlePacket(Packet packet)
        {
            packetHandlers.TryGetValue(packet.packetType, out Action<Packet> action);
            action.Invoke(packet);
        }
    }
}
