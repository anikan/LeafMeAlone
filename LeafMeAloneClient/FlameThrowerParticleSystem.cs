using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    public class FlameThrowerParticleSystem : NormalParticleSystem
    {  
        public float FlameInitSpeed = 40.0f;
        public float FlameAcceleration = 15.0f;
        public float CutoffDist = 10.0f;

        public FlameThrowerParticleSystem(float angle = 320f, float initSpd = 40.0f, float initAccel = 15.0f,float cutoff = 10.0f, float size = 1.0f, float range = 20f, float cutoffSpeed = 0.2f) : 
            base(Constants.FireTexture,
                Vector3.Zero +
                Vector3.TransformCoordinate(Constants.PlayerToToolOffset, Matrix.Identity), // origin
                Vector3.UnitZ * initSpd, // acceleration
                Vector3.UnitZ * initAccel, // initial speed
                false, // cutoff all colors
                true, // no backward particle prevention
                angle,//320.0f, // cone radius, may need to adjust whenever acceleration changes
                size, // initial delta size
                cutoff, // cutoff distance
                cutoffSpeed, // cutoff speed
                0.075f, // enlarge speed,,
                range
                //Tool.Thrower.Range
                )
        {
            FlameInitSpeed = initSpd;
            FlameAcceleration = initAccel;
            CutoffDist = cutoff;
        }
    }
}
