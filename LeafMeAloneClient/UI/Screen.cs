using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.UI
{
    public static class Screen
    {
        public static int Width => GraphicsRenderer.Form.ClientSize.Width;
        public static int Height => GraphicsRenderer.Form.ClientSize.Height;
    }
}
