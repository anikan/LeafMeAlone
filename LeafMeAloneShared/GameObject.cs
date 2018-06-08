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

        public TeamSection section;
        public TeamSection prevSection;

        // Is this object burning?
        public bool Burning
        {
            // Just see if burn frames is more than zero.
            get => burnFrames > 0;

            // To set burning, just set the burn frames to 1 or 0.
            set
            {
                if (Burnable)
                {
                    burnFrames = value ? 1 : 0;
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
        public string Name = "";

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

        /// <summary>
        /// Gets the distance of this object to the player.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>The distance from the object to the player</returns>
        public float GetDistanceToTool(Transform toolTransform)
        {

            // Get the vector between the two.
            Vector3 VectorBetween = Transform.Position - toolTransform.Position;
            VectorBetween.Y = 0.0f;

            // Calculate the length.
            return VectorBetween.Length();

        }

        public bool IsWithinToolRange(Transform toolTransform, ToolType toolType, ToolMode toolMode)
        {

            //  Console.WriteLine("Checking object with ID " + Id  + " and type " + this.GetType().ToString());
            // Get the player's equipped tool.
            ToolInfo toolInfo = Tool.GetToolInfo(toolType);

            float toolRange = toolInfo.Range;

            if (toolType == ToolType.BLOWER && toolMode == ToolMode.SECONDARY)
            {
                toolRange *= 0.5f;
            }

            // Check if the leaf is within range of the player.
            if (GetDistanceToTool(toolTransform) <= toolRange)
            {
                // Get the forward vector of the player.
                Vector3 ToolForward = toolTransform.Forward;
                ToolForward.Y = 0.0f;

                // Get the vector from the tool to the leaf.
                Vector3 ToolToObject = Transform.Position - toolTransform.Position;
                ToolToObject.Y = 0.0f;

                // Get dot product of the two vectors and the product of their magnitudes.
                float dot = Vector3.Dot(ToolForward, ToolToObject);
                float mag = ToolForward.Length() * ToolToObject.Length();

                // Calculate the angle between the two vectors.
                float angleBetween = (float)Math.Acos(dot / mag);
                angleBetween *= (180.0f / (float)Math.PI);

                //  Console.WriteLine(string.Format("{0} {1}: Angle between is {2}, must be {3} before hit", this.GetType().ToString(), Id, angleBetween, equippedToolInfo.ConeAngle / 2.0f));

                // Return true if the leaf is within the cone angle, false otherwise. 
                return (angleBetween <= (toolInfo.ConeAngle / 2.0f));

            }

            return false;
        }
    }
}
