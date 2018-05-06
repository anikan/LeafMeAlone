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
        }

        public override void Update(float deltaTime)
        {
        }

        /// <summary>
        /// Add this GameObject to the server's list of GameObjects and set the id.
        /// </summary>
        public void Register()
        {
            Id = GameServer.instance.gameObjectDict.Count();

            GameServer.instance.gameObjectDict.Add(Id, this);
        }
    }
}
