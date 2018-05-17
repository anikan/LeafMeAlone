using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    public class FlameThrowerParticleSystem : ParticleSystem
    {  
        public static Vector3 PlayerToFlamethrowerOffset = new Vector3(1.8f, 3.85f, 3.0f);
        public float FlameInitSpeed = 40.0f;
        public float FlameAcceleration = 15.0f;

        public FlameThrowerParticleSystem(float initSpd = 40.0f, float initAccel = 15.0f) : 
            base(ParticleSystemType.FIRE,
                Vector3.Zero +
                Vector3.TransformCoordinate(PlayerToFlamethrowerOffset, Matrix.Identity), // origin
                Vector3.UnitZ * initSpd, // acceleration
                Vector3.UnitZ * initAccel, // initial speed
                false, // cutoff all colors
                false, // no backward particle prevention
                Tool.Thrower.ConeAngle * 10,//320.0f, // cone radius, may need to adjust whenever acceleration changes
                1.0f, // initial delta size
                10f, // cutoff distance
                0.2f, // cutoff speed
                0.075f, // enlarge speed,
                Tool.Thrower.Range
                )
        {
            FlameInitSpeed = initSpd;
            FlameAcceleration = initAccel;
        }
    }
}
