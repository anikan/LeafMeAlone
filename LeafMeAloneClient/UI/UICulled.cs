using System.Drawing;
using SpriteTextRenderer;

namespace Client.UI
{

    /// <summary>
    /// Not being used, but good for debugging.
    /// </summary>
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
