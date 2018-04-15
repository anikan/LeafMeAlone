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

        public InputManager(Player userPlayer)
        {
            InputMap = new Dictionary<Key, Func<int>>{
                { Key.W, () => { userPlayer.Move(Player.MoveDirection.NORTH); return 0; } },
                { Key.A, () => { userPlayer.Move(Player.MoveDirection.WEST); return 0; } },
                { Key.S, () => { userPlayer.Move(Player.MoveDirection.SOUTH); return 0; } },
                { Key.D, () => { userPlayer.Move(Player.MoveDirection.EAST); return 0; } }
            };

        }

        public void OnKeyPress(Key key)
        {
            InputMap.TryGetValue(key, out Func<int> keyAction);
            keyAction();
        }

        public void OnMouseMove(Vector2 mousePosition)
        {

        }
        // Detect a keypress
        // If it is a 
    }
}
