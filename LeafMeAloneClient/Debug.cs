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
        public static TextBox debugBox;
        public static void Init(TextBox textbox)
        {
            debugBox = textbox;
        }

        public static void Log(string msg)
        {
            if (debugBox.Lines.Length > 10000)
                debugBox.Text = String.Empty;
            debugBox.AppendText(msg + Environment.NewLine);
        }
    }
}
