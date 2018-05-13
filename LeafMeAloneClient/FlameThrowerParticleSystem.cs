using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Client
{
    public class FlameThrowerParticleSystem : ParticleSystem
    {
        public FlameThrowerParticleSystem() : 
            base(ParticleSystemType.FIRE,
                Vector3.Zero +
                Vector3.TransformCoordinate(GraphicsManager.PlayerToFlamethrowerOffset, Matrix.Identity), // origin
                Vector3.UnitZ * GraphicsManager.FlameInitSpeed, // acceleration
                Vector3.UnitZ * GraphicsManager.FlameAcceleration, // initial speed
                false, // cutoff all colors
                false, // no backward particle prevention
                320.0f, // cone radius, may need to adjust whenever acceleration changes
                1.0f, // initial delta size
                10f, // cutoff distance
                0.2f, // cutoff speed
                0.075f // enlarge speed
                )
        {
        }
    }
}
