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

        public GameObjectServer() : base()
        {

        }

        public GameObjectServer(Transform startTransform) : base(startTransform)
        {

        }

        public override void Draw()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
