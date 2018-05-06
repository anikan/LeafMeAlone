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

    public enum ToolMode
    {
        NONE,
        PRIMARY,
        SECONDARY
    }

    public struct ToolInfo
    {

        public float ConeAngle;
        public float Range;
        public float Force;
        public float Damage;

        public ToolInfo(float angle, float range, float force, float damage)
        {
            this.ConeAngle = angle;
            this.Range = range;
            this.Force = force;
            this.Damage = damage;
        }
    }

    public static class Tool
    {

        private const float ThrowerAngle = 20.0f;
        private const float ThrowerRange = 2.0f;
        private const float ThrowerForce = 0.0f;
        private const float ThrowerDamage = 1.0f;

        private const float BlowerAngle = 45.0f;
        private const float BlowerRange = 3.0f;
        private const float BlowerForce = 0.5f;
        private const float BlowerDamage = 0.0f;

        public static ToolInfo Thrower = new ToolInfo(ThrowerAngle, ThrowerRange, ThrowerForce, ThrowerDamage);
        public static ToolInfo Blower = new ToolInfo(BlowerAngle, BlowerRange, BlowerForce, ThrowerDamage);

        private static Dictionary<ToolType, ToolInfo> ToolMap = new Dictionary<ToolType, ToolInfo>
        {

            {ToolType.BLOWER, Blower},
            {ToolType.THROWER, Thrower}

        };

        public static ToolInfo GetToolInfo(ToolType type)
        {
            return ToolMap[type];
        }
    }
}
