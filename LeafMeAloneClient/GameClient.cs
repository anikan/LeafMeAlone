using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared;
using SlimDX;

namespace Client
{
    static class GameClient
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            Player core = new Player(new Vector3(1,1,1));
            InputManager inputManager = new InputManager(core);
            while (true)
            {
                inputManager.OnKeyPress(SlimDX.DirectInput.Key.W);
            }
        }
    }
}
