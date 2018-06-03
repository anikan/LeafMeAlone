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
    public class UICulled : UI
    {
        //how many culled objects are there?
        public static int Culled = 0;
        

        /// <summary>
        /// Create UI.
        /// </summary>
        public UICulled() : base("", UIManagerSpriteRenderer.TextType.BOLD,
            RectangleF.Empty, TextAlignment.Top | TextAlignment.Left, Color.White)
        {

        }

        /// <summary>
        /// Update every frame.
        /// </summary>
        public override void Update()
        {
            UIText.Text = "Culled: " + Culled;
        }
    }
}
