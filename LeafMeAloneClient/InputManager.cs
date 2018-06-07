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
        public Dictionary<Keys, Action> KeyInputMap;
        public Dictionary<MouseButtons, Action> MouseButtonMap;

        // List of the keys that are currently being pressed.
        public List<Keys> KeysPressed;
        public MouseButtons MouseButtonPressed = MouseButtons.None;

        public Vector2 lastMousePos;
        public PlayerClient userPlayer;

        /// <summary>
        /// Constructor for the input manager. Should take in a player that will respond to input events.
        /// </summary>
        /// <param name="userPlayer"></param>
        public InputManager(PlayerClient userPlayer)
        {

            this.userPlayer = userPlayer;

            // Initialize structures.
            KeysPressed = new List<Keys>();

            // Dictionary to keep track of what functions should be called by what key presses.
            KeyInputMap = new Dictionary<Keys, Action>
            {
                {Keys.W, () => { userPlayer.RequestMove(new Vector2(0.0f, 1.0f)); } },
                {Keys.A, () => { userPlayer.RequestMove(new Vector2(-1.0f, 0.0f));  } },
                {Keys.S, () => { userPlayer.RequestMove(new Vector2(0.0f, -1.0f)); } },
                {Keys.D, () => { userPlayer.RequestMove(new Vector2(1.0f, 0.0f));  } },
                {Keys.D1, () => { userPlayer.RequestToolEquip(ToolType.BLOWER); } },
                {Keys.D2, () => { userPlayer.RequestToolEquip(ToolType.THROWER);  } },
                {Keys.Space, () => { userPlayer.RequestCycleTool(); } },
                {Keys.Q, () => { userPlayer.RequestCycleTool();  } },
                {Keys.Tab, () => { GlobalUIManager.} }
            };

            // Dictionary to keep track of what functions should be called by what mouse presses
            MouseButtonMap = new Dictionary<MouseButtons, Action>
            {

                {MouseButtons.Left, () => { userPlayer.RequestUsePrimary(); } },
                {MouseButtons.Right, () => { userPlayer.RequestUseSecondary(); } }

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
                Keys Key = KeysPressed[i];

                // Check if the key is in the input map.
                if (KeyInputMap.TryGetValue(Key, out Action KeyAction))
                {
                    // If in the map, call the associated action.
                    KeyAction();
                }
            }

            // Check if the active mouse button is mapped.
            if (MouseButtonMap.TryGetValue(MouseButtonPressed, out Action MouseAction))
            {
                // Call the mapped function.
                MouseAction();
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

            if (keyArg.KeyCode == Keys.Q || keyArg.KeyCode == Keys.Space)
            {
                userPlayer.StoppedRequesting = true;
            }
        }

        /// <summary>
        /// Called when a mouse button is pressed down.
        /// </summary>
        /// <param name="ignore"> Ignored. </param>
        /// <param name="arg"> Information about the mouse button pressed. </param>
        public void OnMouseDown(object ignore, MouseEventArgs arg)
        {
            // Set the mouse button that's been pressed this frame.
            MouseButtonPressed = arg.Button;
        }

        /// <summary>
        /// Called when the mouse button is lifted.
        /// </summary>
        /// <param name="ignore"> Ignored. </param>
        /// <param name="arg"> Information about the mouse button lifted. </param>
        public void OnMouseUp(object ignore, MouseEventArgs arg)
        {
            // Check if the lifted button is the active one.
            if (MouseButtonPressed == arg.Button)
            {
                // If so, there's no mouse button being pressed anymore this frame.
                MouseButtonPressed = MouseButtons.None;
            }
        }

        // Mouse movement event handler.
        public void OnMouseMove(object sender, MouseEventArgs args)
        {
            // Get the new position of the mouse.
            Vector2 mousePos = new Vector2(args.X, args.Y);

            userPlayer.RequestLookAtScreenSpace(mousePos);

            // Updaate last mouse position.
           // lastMousePos = mousePos;
        }
    }
}
