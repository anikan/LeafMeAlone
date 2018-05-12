using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{

    // Type of tool the player is using.
    public enum ToolType
    {
        NONE,
        BLOWER,
        THROWER
    }

    // Mode for left/right tool clicks, or maybe any special (power-up) modes.
    public enum ToolMode
    {
        NONE,
        PRIMARY,
        SECONDARY
    }

    // Struct that stores relevant tool information.
    public struct ToolInfo
    {

        // Angle of this tool, for a cone in front of the player.
        public float ConeAngle;

        // Range of the tool.
        public float Range;
        
        // Force this tool should apply on the object (may be zero).
        public float Force;

        // Damage this tool should do to any hit objects (may be zero).
        public float Damage;

        // Fill in the tool info.
        public ToolInfo(float angle, float range, float force, float damage)
        {
            this.ConeAngle = angle;
            this.Range = range;
            this.Force = force;
            this.Damage = damage;
        }
    }

    /// <summary>
    /// Class to store tool information and constants.
    /// </summary>
    public static class Tool
    {

        // Flamethrower tool information. 
        private const float ThrowerAngle = 20.0f;
        private const float ThrowerRange = 2.0f;
        private const float ThrowerForce = 0.0f;
        private const float ThrowerDamage = 1.0f;

        // Leafblower tool information.
        private const float BlowerAngle = 10.0f;
        private const float BlowerRange = 15.0f;
        private const float BlowerForce = 200.0f;
        private const float BlowerDamage = 0.0f;

        // Create a flamethrower struct to store flamethrower info.
        public static ToolInfo Thrower = new ToolInfo(ThrowerAngle, ThrowerRange, ThrowerForce, ThrowerDamage);

        // Create a leaf blower struct to store blower info.
        public static ToolInfo Blower = new ToolInfo(BlowerAngle, BlowerRange, BlowerForce, ThrowerDamage);

        // Dictionary mapping tool types to structs.
        private static Dictionary<ToolType, ToolInfo> ToolMap = new Dictionary<ToolType, ToolInfo>
        {

            {ToolType.BLOWER, Blower},
            {ToolType.THROWER, Thrower},
            {ToolType.NONE, new ToolInfo() }

        };
        
        // Gets tool information from a tool type.
        public static ToolInfo GetToolInfo(ToolType type)
        {
            
            if (ToolMap.TryGetValue(type, out ToolInfo info))
            {
                return ToolMap[type];

            }
            else
            {
                Console.WriteLine("Error: Could not get tool type " + type.ToString() + " from tool info map.");
                return new ToolInfo();
            }
        }
    }
}
