using Shared;
using Shared.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    /// <summary>
    /// Class which handles mapping the different packet types to client actions
    /// </summary>
    class ClientPacketHandler
    {
        private Dictionary<PacketType, Action<BasePacket>> packetHandlers;
        private GameClient client;

        /// <summary>
        /// Defines the different actions which the client will take upon recieving a packet, and defines 
        /// them as a dictionary that hashes by packet type
        /// </summary>
        /// <param name="client">The game client to fire callbacks for</param>
        public ClientPacketHandler(GameClient client)
        {
            this.client = client;

            // What to do during an update
            void UpdateAction(BasePacket p)
            {
                NetworkedGameObjectClient packetObject = client.GetObjectFromPacket((IIdentifiable)p);
                packetObject.UpdateFromPacket(p);
            }

            // What to do during a destroy
            void DestroyAction(BasePacket p)
            {
                NetworkedGameObjectClient packetObject = client.GetObjectFromPacket((IIdentifiable)p);
                packetObject.Destroy();
            }
            
            // What to do when creating an object
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

        /// <summary>
        /// Fires a callback into the game client to handle a packet
        /// </summary>
        /// <param name="packet">The packet to handle</param>
        internal void HandlePacket(BasePacket packet)
        {
            packetHandlers.TryGetValue(packet.packetType, out Action<BasePacket> action);
            action.Invoke(packet);
        }
    }
}
