using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    static class AnimationManager
    {
        private static List<PlayerClient> _players;

        public static void Init()
        {
            _players = new List<PlayerClient>();

            // preload animations
            GraphicsManager.DictGeometry[Constants.PlayerIdleAnim] = new Geometry(Constants.PlayerIdleAnim, true);
            GraphicsManager.DictGeometry[Constants.PlayerWalkAnim] = new Geometry(Constants.PlayerWalkAnim, true);
        }

        public static void Update()
        {

        }

        public static void AddPlayer( ref PlayerClient player )
        {
            _players.Add(player);
        }
    }
}
