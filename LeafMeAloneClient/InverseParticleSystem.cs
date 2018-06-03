using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Client
{
    class InverseParticleSystem : ParticleSystem
    {
        public InverseParticleSystem(string TexturePath, Vector3 init_pos, Vector3 acceleration, Vector3 init_velocity,
            bool alpha_cutoff_only = false, bool disable_rewind = true, float cone_radius = 20, float initial_size = 1, 
            float cutoff_dist = 10, float cutoff_speed = 0.2f, float enlarge_speed = 0.075f, float stop_dist = 50, 
            int emissionrate = 2, int maxparticles = 100) 
            
            : base(TexturePath, init_pos, acceleration, init_velocity, alpha_cutoff_only, disable_rewind, cone_radius, 
                initial_size, cutoff_dist, cutoff_speed, enlarge_speed, stop_dist, emissionrate, maxparticles)
        {

        }


    }
}
