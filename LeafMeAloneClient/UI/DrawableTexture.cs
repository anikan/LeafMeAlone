using SlimDX;

namespace Client.UI
{
    public class DrawableTexture
    {
        //Helper Class for Drawable Textures.


        public string View;
        public Vector2 Position, Size;
        public double Rotation;
        public bool Enabled = true;

        public DrawableTexture(string view, Vector2 position, Vector2 size, double rotation)
        {
            View = view;
            Position = position;
            Size = size;
            Rotation = rotation;
        }
    }
}