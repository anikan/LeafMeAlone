using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{

    /// <summary>
    /// Class for an actual player of the game.
    /// </summary>
    public interface IPlayer : IObject
    {
        bool Dead { get; set; }

        ToolType ToolEquipped { get; set; }

        ToolMode ActiveToolMode { get; set; }

        void UpdateFromPacket(PlayerPacket packet);

    }
}
