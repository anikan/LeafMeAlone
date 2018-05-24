using Shared;
using Shared.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class ClientPacketHandler
    {
        private Dictionary<PacketType, Action<BasePacket>> packetHandlers;
        private GameClient client;

        public ClientPacketHandler(GameClient client)
        {
            this.client = client;

            void UpdateAction(BasePacket p)
            {
                NetworkedGameObjectClient packetObject = client.GetObjectFromPacket((IIdentifiable)p);
                packetObject.UpdateFromPacket(p);
            }

            void DestroyAction(BasePacket p)
            {
                NetworkedGameObjectClient packetObject = client.GetObjectFromPacket((IIdentifiable)p);
                packetObject.Destroy();
            }
            
            void CreateObjectAction(BasePacket p)
            {
                client.CreateObjectFromPacket(((CreateObjectPacket)p));
            }

            packetHandlers = new Dictionary<PacketType, Action<BasePacket>>()
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

        internal void HandlePacket(BasePacket packet)
        {
            packetHandlers.TryGetValue(packet.packetType, out Action<BasePacket> action);
            action.Invoke(packet);
        }
    }
}
