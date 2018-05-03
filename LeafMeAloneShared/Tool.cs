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
        BLOWER,
        THROWER
    }

    public struct ToolInfo
    {

        public float ConeAngle;
        public float Range;
        public float Force;

        public ToolInfo(float angle, float range, float force)
        {
            this.ConeAngle = angle;
            this.Range = range;
            this.Force = force;
        }
    }

    public static class Tool
    {

        private const float ThrowerAngle = 20.0f;
        private const float ThrowerRange = 2.0f;
        private const float ThrowerForce = 0.0f;

        private const float BlowerAngle = 45.0f;
        private const float BlowerRange = 3.0f;
        private const float BlowerForce = 0.5f;

        public static ToolInfo Thrower = new ToolInfo(ThrowerAngle, ThrowerRange, ThrowerForce);
        public static ToolInfo Blower = new ToolInfo(BlowerAngle, BlowerRange, BlowerForce);

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
