using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    public class LeafBlowerParticleSystem : ParticleSystem
    {

        public static Vector3 PlayerToWindOffset = new Vector3(1.8f, 3.85f, 3.0f);
        public static float WindInitSpeed = 60.0f;
        public static float WindAcceleration = -30.0f;
        public static float WindStopDistance = 60.0f;
        public static Vector3 WindDirection = Vector3.UnitX;

        public LeafBlowerParticleSystem(): base(ParticleSystemType.WIND,
            Vector3.Zero +
            Vector3.TransformCoordinate(PlayerToWindOffset, Matrix.Identity), // origin
            WindDirection * WindAcceleration, // acceleration
            WindDirection * WindInitSpeed, // initial speed
            true, // cutoff alpha only
            true, // prevent backward flow 
            Tool.Blower.ConeAngle*30,//800.0f, // cone radius
            1.0f, // initial delta size
            0f, // cutoff distance
            0.5f, // cutoff speed
            0.1f, // enlarge speed
            Tool.Blower.Range//.WindStopDistance // stop dist
            )
        {
          
        }
    }
}
