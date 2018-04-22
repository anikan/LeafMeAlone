using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Shared
{
    public abstract class GameObject
    {
        /// <summary>
        /// ID is given to gameobjects to distinguish between them.
        /// </summary>
        private int id;

        /// <summary>
        /// Name can be given to gameobjects for debugging purposes.
        /// </summary>
        public string Name;

        public Transform Transform;

        public abstract void Update();
        public abstract void Draw();


        protected GameObject()
        {
            Transform.Direction = new Vector3(0, 0, 0);
            Transform.Position = new Vector3(0, 0, 0);
            Transform.Scale = new Vector3(1, 1, 1);
        }
        
        /// <summary>
        /// Get the current ID of the GameObject
        /// </summary>
        /// <returns></returns>
        public int GetId()
        {
            return id;
        }

        /// <summary>
        /// Set the current ID of the GameObject
        /// </summary>
        /// <param name="id"></param>
        public void SetId(int myId)
        {
            id = myId;
        }

    }
}
