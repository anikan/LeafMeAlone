using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AntTweakBar;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DirectWrite;
using SpriteTextRenderer;
using SpriteRenderer = SpriteTextRenderer.SlimDX.SpriteRenderer;
using TextBlockRenderer = SpriteTextRenderer.SlimDX.TextBlockRenderer;

namespace Client
{
    public class UIManager
    {


        public static Dictionary<string, Bar> ActiveUI = new Dictionary<string, Bar>();

        public static Bar Create(string name, Size size, Point location)
        {
            var b = new Bar(GraphicsRenderer.BarContext)
            {
                Label = name,
                Contained = true,
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

        public UIManager()
        {

        }
    }


    public class Tex
    {
        public Texture2D SdxTexture;
        public ShaderResourceView SrvTexture;
    }

    public class DrawableString
    {
        public string Text;
        public UIManager2.TextType Type;
        public Vector2 Position;
        public Color Color;

        public DrawableString(string text, UIManager2.TextType type, Vector2 position, Color color)
        {
            this.Text = text;
            this.Type = type;
            this.Position = position;
            this.Color = color;
        }
    }


    public static class UIManager2
    {
        public enum TextType
        {
            BOLD,
            NORMAL,
            COMIC_SANS
        }

        public static SpriteRenderer SpriteRenderer;
        public static Dictionary<TextType, TextBlockRenderer> TextRenderers = new Dictionary<TextType, TextBlockRenderer>();
        public static Dictionary<string, Tex> DrawableImages = new Dictionary<string, Tex>();

        private static List<DrawableString> textPerFrame = new List<DrawableString>();

        public static void Init()
        {

            SpriteRenderer = new SpriteRenderer(GraphicsRenderer.Device);
            //new SpriteTextRenderer.SlimDX.TextBlockRenderer(Client.sprite, "Arial", FontWeight.Bold, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 16);

        }

        public static void DrawText(string text, TextType type,Vector2 position,Color color)
        {
            if (!TextRenderers.ContainsKey(type))
            {
                switch (type)
                {
                    case TextType.BOLD:
                        TextRenderers[type] = new TextBlockRenderer(SpriteRenderer, "Arial", FontWeight.Bold, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 16);
                        break;
                    case TextType.NORMAL:
                        TextRenderers[type] = new TextBlockRenderer(SpriteRenderer, "Arial", FontWeight.Normal, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 16);
                        break;
                    case TextType.COMIC_SANS:
                        TextRenderers[type] = new TextBlockRenderer(SpriteRenderer, "Comic Sans", FontWeight.Normal, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 16);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            TextRenderers[type].DrawString(text, position, new Color4(color));
            //sprite.Draw(srvTexture, new Vector2(300, 170), new Vector2(150, 150), new Vector2(75, 75), new Radians(0 / 1000.0), CoordinateType.Absolute);
        }

        public static DrawableString DrawTextContinuous(string text, TextType type, Vector2 position, Color color)
        {
            DrawableString d = new DrawableString(text,type,position,color);
            textPerFrame.Add(d);
            return d;
        }

        public static void RemoveTextContinuous(DrawableString d)
        {
            textPerFrame.Remove(d);
        }


        public static void Update()
        {
            foreach (DrawableString str in textPerFrame)
            {
                DrawText(str.Text,str.Type,str.Position,str.Color);
            }
        }


    }


}
