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
    /// <summary>
    /// Handles user input and calls input mapped functions. 
    /// </summary>
    class InputManager
    {
        // Maps keys presesd to functions they should call.
        public Dictionary<Keys, Action<int>> InputMap;

        // List of the keys that are currently being pressed.
        public List<Keys> KeysPressed;

        /// <summary>
        /// Constructor for the input manager. Should take in a player that will respond to input events.
        /// </summary>
        /// <param name="userPlayer"></param>
        public InputManager(PlayerClient userPlayer)
        {
            // Initialize structures.
            InputMap = new Dictionary<Keys, Action<int>>();
            KeysPressed = new List<Keys>();

            // Dictionary to keep track of what functions should be called by what key presses.
            InputMap = new Dictionary<Keys, Action<int>> {
                {Keys.W, (int dir) => { userPlayer.RequestMove(dir * new Vector2(0.0f, 1.0f)); } },
                {Keys.A, (int dir) => { userPlayer.RequestMove(dir * new Vector2(-1.0f, 0.0f));  } },
                {Keys.S, (int dir) => { userPlayer.RequestMove(dir * new Vector2(0.0f, -1.0f)); } },
                {Keys.D, (int dir) => { userPlayer.RequestMove(dir * new Vector2(1.0f, 0.0f));  } }
            };
        }

        /// <summary>
        /// Checks all keys pressed this frame and calls their functions from the input map.
        /// </summary>
        public void Update()
        {

            // Iterate through all keys pressed.
            for (int i = 0; i < KeysPressed.Count; i++)
            {

                // The key that was pressed in the list.
                Keys key = KeysPressed[i];

                // Check if the key is in the input map.
                if (InputMap.TryGetValue(key, out Action<int> keyAction))
                {
                    // If in the map, call the associated action.
                    keyAction(1);
                }
            }
        }

        /// <summary>
        /// Called whenever a key is pressed down.
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="keyArg">Information about the key that was pressed.</param>
        public void OnKeyDown(object ignore, KeyEventArgs keyArg)
        {
            // If the key isn't already in the pressed list, add it.
            if (!KeysPressed.Contains(keyArg.KeyCode))
            {
                // Add key to the list of keys pressed.
                KeysPressed.Add(keyArg.KeyCode);
            }


        }

        /// <summary>
        /// called whenever a key is lifed.
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="keyArg">Information about the key that was pressed</param>
        public void OnKeyUp(object ignore, KeyEventArgs keyArg)
        {

            // Remove the key if it was in the keys pressed list.
            if (KeysPressed.Contains(keyArg.KeyCode))
            {
                KeysPressed.Remove(keyArg.KeyCode);
            }
        }

        // Mouse movement event handler.
        public void OnMouseMove(Vector2 mousePosition)
        {
            // TODO
        }
    }
}
