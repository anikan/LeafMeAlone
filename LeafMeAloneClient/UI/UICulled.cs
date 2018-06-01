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
    public class UICulled
    {
        //how many culled objects are there?
        public static int Culled = 0;

        //UI Element.
        private readonly DrawableString CulledUI;

        /// <summary>
        /// Create UI.
        /// </summary>
        public UICulled()
        {
            CulledUI = UIManagerSpriteRenderer.DrawTextContinuous("", UIManagerSpriteRenderer.TextType.BOLD,
                new RectangleF(0, 0, GraphicsRenderer.Form.ClientSize.Width, GraphicsRenderer.Form.ClientSize.Height), TextAlignment.HorizontalCenter | TextAlignment.Bottom, Color.White);

        }

        /// <summary>
        /// Update every frame.
        /// </summary>
        public void Update()
        {
            CulledUI.Text = "Objects Culled This Frame: " + Culled;
        }
    }
}
