using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PacketFactory
    {

        public static ObjectPacket CreateObjectPacket(GameObject gameObj)
        {
            IdPacket idPack = new IdPacket(gameObj.Id);
            Vector3 pos = gameObj.Transform.Position;
            Vector3 rot = gameObj.Transform.Rotation;
            return new ObjectPacket(pos.X, pos.Y, pos.Z, rot.Y, gameObj.Burning, idPack);
        }

        public static Packet CreateDestroyPacket(GameObject gameObj)
        {
            return new DestroyObjectPacket(new IdPacket(gameObj.Id));
        }

        public static CreateObjectPacket CreateCreatePacket(GameObject obj)
        {
            return new CreateObjectPacket(CreateObjectPacket(obj), obj.ObjectType);
        }
    }
}
