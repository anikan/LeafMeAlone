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
    public interface IPlayer : INetworked
    {
        bool UsingTool { get; set; }

        Transform GetTransform();
        void SetTransform(Transform value);

        bool Dead { get; set; }
        PlayerPacket.ToolType ToolEquipped { get; set; }
        void UpdateFromPacket(PlayerPacket packet);
    }
}
