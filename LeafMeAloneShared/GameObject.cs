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

        // Can this object be burned?
        public bool Burnable = false;

        // How many frames this object has consistently been burning for.
        protected int burnFrames = 0;
        protected int blowFrames = 0;

        // Is this object burning?
        public bool Burning
        {
            // Just see if burn frames is more than zero.
            get
            {
                return burnFrames > 0;
            }
            
            // To set burning, just set the burn frames to 1 or 0.
            set
            {
                if (Burnable)
                {
                    burnFrames = (value == true) ? 1 : 0;
                }
                else
                {
                    burnFrames = 0;
                }
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
        public abstract void Die();
          
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
