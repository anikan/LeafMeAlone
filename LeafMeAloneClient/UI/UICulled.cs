using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpriteTextRenderer;

namespace Client.UI
{
    class UICulled
    {
        public static int Culled = 0;
        private readonly DrawableString CulledUI;

        public UICulled()
        {
            CulledUI = UIManagerSpriteRenderer.DrawTextContinuous("", UIManagerSpriteRenderer.TextType.BOLD,
                new RectangleF(0, 0, GraphicsRenderer.Form.ClientSize.Width, GraphicsRenderer.Form.ClientSize.Height), TextAlignment.HorizontalCenter | TextAlignment.Bottom, Color.White);

        }

        public void Update()
        {
            CulledUI.Text = "Objects Culled This Frame: " + Culled;
        }
    }
}
