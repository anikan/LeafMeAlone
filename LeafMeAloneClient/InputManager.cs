using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;
using System.Windows.Forms;

namespace Client
{
    class InputManager
    {
        public Dictionary<char, Func<int>> InputMap;

        /// <summary>
        /// Constructor for the input manager. Should take in a player that will respond to input events.
        /// </summary>
        /// <param name="userPlayer"></param>
        public InputManager(Player userPlayer)
        {
            // Dictionary to keep track of what functions should be called by what key presses.
            InputMap = new Dictionary<char, Func<int>>{
                { 'w', () => { userPlayer.Move(Player.MoveDirection.NORTH); return 0; } },
                { 'a', () => { userPlayer.Move(Player.MoveDirection.WEST); return 0; } },
                { 's', () => { userPlayer.Move(Player.MoveDirection.SOUTH); return 0; } },
                { 'd', () => { userPlayer.Move(Player.MoveDirection.EAST); return 0; } }
            };

        }

        // Key press event handler. Calls functions from the input map.
        public void OnKeyPress(object ignored, KeyPressEventArgs keyArg)
        {
            InputMap.TryGetValue(keyArg.KeyChar, out Func<int> keyAction);
            keyAction();
        }

        // Mouse movement event handler.
        public void OnMouseMove(Vector2 mousePosition)
        {
            // TODO
        }
    }
}
