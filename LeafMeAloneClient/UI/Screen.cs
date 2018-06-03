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
        public static float HorizontalCenter => GraphicsRenderer.Form.ClientSize.Width / 2f;
        public static int Height => GraphicsRenderer.Form.ClientSize.Height;
        public static float VerticalCenter => GraphicsRenderer.Form.ClientSize.Height / 2f;
    }
}
