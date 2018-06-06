using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public static class Debug
    {

        //Textbox for debug messages
        public static TextBox debugBox;

        //initialize the debug textbox.
        public static void Init(TextBox textbox)
        {
            debugBox = textbox;
        }

        //log a debug message to the textbox
        public static void Log(string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
            //reset logs if too long.
            if (debugBox.Lines.Length > 10000)
                debugBox.Text = String.Empty;

            //append to textbox.
            debugBox.AppendText(msg + Environment.NewLine);
#endif
        }
    }
}
