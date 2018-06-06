using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    public class LeafBlowerParticleSystem : NormalParticleSystem
    {
        public static float WindInitSpeed = 70.0f;
        public static float WindAcceleration = -30.0f;
        public static float WindStopDistance = 60.0f;
        public static Vector3 WindDirection = Vector3.UnitX;

        public LeafBlowerParticleSystem(): base(Constants.WindTexture,
            Vector3.Zero +
            Vector3.TransformCoordinate(Constants.PlayerToToolOffset, Matrix.Identity), // origin
            WindDirection * WindAcceleration, // acceleration
            WindDirection * WindInitSpeed, // initial speed
            true, // cutoff alpha only
            true, // prevent backward flow 
            Tool.Blower.ConeAngle*30,//800.0f, // cone radius
            1.2f, // initial delta size
            0f, // cutoff distance
            0.5f, // cutoff speed
            0.02f, // enlarge speed
            Tool.Blower.Range//.WindStopDistance // stop dist
            )
        {
          
        }
    }
}
