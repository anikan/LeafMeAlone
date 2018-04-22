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
    public interface IPlayer
    {
        bool GetUsingTool();
        void SetUsingTool(bool value);

        bool GetDead();
        void SetDead(bool value);

        PlayerPacket.ToolType GetToolEquipped();
        void SetToolEquipped(PlayerPacket.ToolType value);
    }
}
