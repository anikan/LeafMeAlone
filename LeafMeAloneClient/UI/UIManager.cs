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
using SpriteTextRenderer.SlimDX;
using SpriteRenderer = SpriteTextRenderer.SlimDX.SpriteRenderer;
using TextAlignment = SpriteTextRenderer.TextAlignment;
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
    public class DrawableString2
    {
        public string Text;
        public UIManager2.TextType Type;
        private Rectangle pos;
        private SpriteTextRenderer.TextAlignment alignment;
        public Color Color;

        public DrawableString2(string text, UIManager2.TextType type, Color color, Rectangle pos, TextAlignment alignment)
        {
            this.Text = text;
            this.Type = type;
            this.Color = color;
            this.pos = pos;
            this.alignment = alignment;
        }
    }


    public static class UIManager2
    {
        public enum TextType
        {
            BOLD,
            NORMAL,
            COMIC_SANS,
            MASSIVE
        }

        public static SpriteRenderer SpriteRenderer;
        public static Dictionary<TextType, TextBlockRenderer> TextRenderers = new Dictionary<TextType, TextBlockRenderer>();
        public static Dictionary<string, Tex> DrawableImages = new Dictionary<string, Tex>();

        private static List<DrawableString> textPerFrame = new List<DrawableString>();

        public static void Init()
        {

            SpriteRenderer = new SpriteRenderer(GraphicsRenderer.Device);
         }

        public static void DrawText(string text, TextType type, Vector2 position, Color color)
        {
            EnsureTypeExists(type);
            TextRenderers[type].DrawString(text, position, new Color4(color));
        }

        public static void DrawText(string text, TextType type, Rectangle pos, SpriteTextRenderer.TextAlignment alignment, Color color)
        {
            EnsureTypeExists(type);
            TextRenderers[type].DrawString(text, pos, alignment, new Color4(color));
        }

        public static DrawableString DrawTextContinuous(string text, TextType type, Vector2 position, Color color)
        {
            DrawableString d = new DrawableString(text, type, position, color);
            textPerFrame.Add(d);
            return d;
        }
        public static DrawableString2 DrawTextContinuous(string text, TextType type, Rectangle pos, SpriteTextRenderer.TextAlignment alignment, Color color)
        {
            DrawableString2 d = new DrawableString2(text, type, pos,alignment, color);
            textPerFrame.Add(d);
            return d;
        }

        public static void RemoveTextContinuous(DrawableString d)
        {
            textPerFrame.Remove(d);
        }

        public static Vector2 GetTextWidth(string text, TextType type)
        {
            EnsureTypeExists(type);
            return TextRenderers[type].MeasureString(text).Size.ToVector();
        }

        public static void Update()
        {
            foreach (DrawableString str in textPerFrame)
            {
                DrawText(str.Text, str.Type, str.Position, str.Color);
            }
        }

        private static void EnsureTypeExists(TextType t)
        {
            if (TextRenderers.ContainsKey(t)) return;

            switch (t)
            {
                case TextType.BOLD:
                    TextRenderers[t] = new TextBlockRenderer(SpriteRenderer, "Arial", FontWeight.Bold, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 16);
                    break;
                case TextType.NORMAL:
                    TextRenderers[t] = new TextBlockRenderer(SpriteRenderer, "Arial", FontWeight.Normal, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 16);
                    break;
                case TextType.COMIC_SANS:
                    TextRenderers[t] = new TextBlockRenderer(SpriteRenderer, "Comic Sans", FontWeight.Normal, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 16);
                    break;
                case TextType.MASSIVE:
                    TextRenderers[t] = new TextBlockRenderer(SpriteRenderer, "Arial", FontWeight.Normal, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 50);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }
    }


}
