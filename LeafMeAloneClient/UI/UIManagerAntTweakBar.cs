using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using AntTweakBar;
using SlimDX;
using SpriteTextRenderer.SlimDX;
using TextAlignment = SpriteTextRenderer.TextAlignment;

namespace Client
{
    public class UIManagerAntTweakBar
    {


        public static Dictionary<string, Bar> ActiveUI = new Dictionary<string, Bar>();

        public static Bar Create(string name, Size size, Point location)
        {
            var b = new Bar(GraphicsRenderer.BarContext)
            {
                Label = name,
                Contained = false,
                Size = size,
                Position = location,
            };
            b.SetDefinition("refresh=.1 alpha=0");
            b.ValueColumnWidth = 0;
            if (ActiveUI.ContainsKey(name))
                throw new Exception("Invalid name!");


            ActiveUI[name] = b;
            return b;
        }

        public UIManagerAntTweakBar()
        {

        }
    }



    public class DrawableString
    {
        public string Text;
        public UIManagerSpriteRenderer.TextType Type;
        //public Vector2 Position;
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

    public class DrawableTexture
    {
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
