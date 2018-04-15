using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    class InputManager
    {
        public Dictionary<Key, Func<int>> InputMap;

        /// <summary>
        /// Constructor for the input manager. Should take in a player that will respond to input events.
        /// </summary>
        /// <param name="userPlayer"></param>
        public InputManager(Player userPlayer)
        {

            // Dictionary to keep track of what functions should be called by what key presses.
            InputMap = new Dictionary<Key, Func<int>>{
                { Key.W, () => { userPlayer.Move(Player.MoveDirection.NORTH); return 0; } },
                { Key.A, () => { userPlayer.Move(Player.MoveDirection.WEST); return 0; } },
                { Key.S, () => { userPlayer.Move(Player.MoveDirection.SOUTH); return 0; } },
                { Key.D, () => { userPlayer.Move(Player.MoveDirection.EAST); return 0; } }
            };

        }

        // Key press event handler. Calls functions from the input map.
        public void OnKeyPress(Key key)
        {
            InputMap.TryGetValue(key, out Func<int> keyAction);
            keyAction();
        }

        // Mouse movement event handler.
        public void OnMouseMove(Vector2 mousePosition)
        {
            // TODO
        }
    }
}
