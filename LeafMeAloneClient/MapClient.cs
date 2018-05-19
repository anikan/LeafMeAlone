using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    public class MapClient : NonNetworkedGameObjectClient
    {

        public MapClient(string modelPath = FileManager.DefaultMapModel) : base(modelPath)
        {

            Transform.Position.Y = -10.0f;
            Transform.Scale = new Vector3(100, 1, 100);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            Console.WriteLine("Map position is " + Transform.Position);
        }
    }
}
