using Shared.Packet;
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

        public static ObjectPacket NewObjectPacket(GameObject gameObj)
        {
            IdPacket idPack = new IdPacket(gameObj.Id);
            Vector3 pos = gameObj.Transform.Position;
            Vector3 rot = gameObj.Transform.Rotation;
            return new ObjectPacket(pos.X, pos.Y, pos.Z, rot.Y, gameObj.Burning, gameObj.Health, idPack);
        }

        public static BasePacket NewDestroyPacket(GameObject gameObj)
        {
            return new DestroyObjectPacket(new IdPacket(gameObj.Id));
        }

        public static CreateObjectPacket NewCreatePacket(GameObject obj)
        {
            return new CreateObjectPacket(NewObjectPacket(obj), obj.ObjectType);
        }
    }
}
