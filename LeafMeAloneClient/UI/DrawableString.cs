using System.Drawing;
using SpriteTextRenderer;

namespace Client
{
    public class DrawableString
    {
        //Helper Class for Drawable Strings.

        public string Text;
        public UIManagerSpriteRenderer.TextType Type;
        public Color Color;
        public RectangleF Position;
        public TextAlignment Alignment;

        public DrawableString(string text, UIManagerSpriteRenderer.TextType type, RectangleF position, TextAlignment alignment, Color color)
        {
            this.Text = text;
            this.Type = type;
            this.Color = color;
            this.Position = position;
            this.Alignment = alignment;
        }
    }
}