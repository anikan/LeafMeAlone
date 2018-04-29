using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Server
{
    public class GameObjectServer : GameObject
    {


        public GameObjectServer(ObjectType objectType) : base()
        {
            ObjectType = objectType;
        }

        public GameObjectServer(ObjectType objectType, Transform startPosition) : base(startPosition)
        {
            ObjectType = objectType;
        }

        public override void Draw()
        {
            throw new NotImplementedException();
        }

        public override void Update(float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}
