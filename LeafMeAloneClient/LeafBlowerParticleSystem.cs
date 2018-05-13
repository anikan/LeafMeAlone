using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Client
{
    public class LeafBlowerParticleSystem : ParticleSystem
    {
        public LeafBlowerParticleSystem(): base(ParticleSystemType.WIND,
            new Vector3(-10, -10, 0), // origin
            GraphicsManager.WindDirection * GraphicsManager.WindAcceleration, // acceleration
            GraphicsManager.WindDirection * GraphicsManager.WindInitSpeed, // initial speed
            true, // cutoff alpha only
            true, // prevent backward flow 
            800.0f, // cone radius
            1.0f, // initial delta size
            0f, // cutoff distance
            0.5f, // cutoff speed
            0.1f, // enlarge speed
            GraphicsManager.WindStopDistance // stop dist
            )
        {

        }
    }
}
