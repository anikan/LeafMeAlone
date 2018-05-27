using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Shared
{
    // Type of an object in the game.
    public enum ObjectType
    {
        OTHER,
        ACTIVE_PLAYER,
        PLAYER,
        LEAF,
        MAP,
        TREE
    };

    /// <summary>
    /// Abstract GameObject class, for both GameObjectClient and GameObjectServer to extend
    /// </summary>
    public abstract class GameObject : IObject
    {
        protected int burnFrames = 0;

        public bool Burning
        {
            get
            {
                return burnFrames > 0;
            }
            set
            {
                burnFrames = (value == true) ? 1 : 0;
            }
        }
        public float Health;

        public ObjectType ObjectType;

        /// <summary>
        /// Name can be given to gameobjects for debugging purposes.
        /// </summary>
        public string Name;

        public Transform Transform;
        public int Id { get; set; }

        public abstract void Update(float deltaTime);
        public abstract void Destroy();
          
        protected GameObject()
        {
            Transform = new Transform();
        }

        protected GameObject(Transform startTransform)
        {
            Transform = startTransform;
        }
    }
}
