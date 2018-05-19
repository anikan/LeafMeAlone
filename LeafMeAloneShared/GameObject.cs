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
        MAP
    };

    /// <summary>
    /// Abstract GameObject class, for both GameObjectClient and GameObjectServer to extend
    /// </summary>
    public abstract class GameObject : INetworked
    {

        public bool Burning;
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
            Transform EmptyTransform = new Transform();

            EmptyTransform.Rotation = new Vector3(0, 0, 0);
            EmptyTransform.Position = new Vector3(0, 0, 0);
            EmptyTransform.Scale = new Vector3(1, 1, 1);

            Transform = EmptyTransform;
        }

        protected GameObject(Transform startTransform)
        {
            Transform = startTransform;
        }
    }
}
