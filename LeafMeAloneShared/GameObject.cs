using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Shared
{
    public abstract class GameObject : INetworked
    {

        /// <summary>
        /// Name can be given to gameobjects for debugging purposes.
        /// </summary>
        public string Name;

        protected Transform Transform;
        public int Id { get; set; }

        public abstract void Update();
        public abstract void Draw();


        protected GameObject()
        {
            Transform.Direction = new Vector3(0, 0, 0);
            Transform.Position = new Vector3(0, 0, 0);
            Transform.Scale = new Vector3(1, 1, 1);
        }
    }
}
