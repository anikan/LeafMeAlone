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

        public InputManager(Player userPlayer)
        {
            InputMap = new Dictionary<char, Func<int>>{
                { 'w', () => { userPlayer.Move(Player.MoveDirection.NORTH); return 0; } },
                { 'a', () => { userPlayer.Move(Player.MoveDirection.WEST); return 0; } },
                { 's', () => { userPlayer.Move(Player.MoveDirection.SOUTH); return 0; } },
                { 'd', () => { userPlayer.Move(Player.MoveDirection.EAST); return 0; } }
            };

        }

        public void OnKeyPress(object ignored, KeyPressEventArgs keyArg)
        {
            InputMap.TryGetValue(keyArg.KeyChar, out Func<int> keyAction);
            keyAction();
        }

        public void OnMouseMove(Vector2 mousePosition)
        {

        }
        // Detect a keypress
        // If it is a 
    }
}
