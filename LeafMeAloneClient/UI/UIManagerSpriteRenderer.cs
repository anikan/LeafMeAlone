using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Shared;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DirectWrite;
using SpriteTextRenderer;
using SpriteRenderer = SpriteTextRenderer.SlimDX.SpriteRenderer;
using TextAlignment = SpriteTextRenderer.TextAlignment;
using TextBlockRenderer = SpriteTextRenderer.SlimDX.TextBlockRenderer;

namespace Client
{
    public static class UIManagerSpriteRenderer
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
        public static Dictionary<string, ShaderResourceView> DrawableImages = new Dictionary<string, ShaderResourceView>();

        private static List<DrawableString> textPerFrame = new List<DrawableString>();
        private static List<DrawableTexture> texturesPerFrame = new List<DrawableTexture>();

        public static void Init()
        {
            SpriteRenderer = new SpriteRenderer(GraphicsRenderer.Device);
        }
        #region Text
        public static void DrawText(string text, TextType type, Vector2 position, Color color)
        {
            EnsureTypeExists(type);
            TextRenderers[type].DrawString(text, position, new Color4(color));
        }

        public static void DrawText(string text, TextType type, RectangleF pos, TextAlignment alignment, Color color)
        {
            EnsureTypeExists(type);
            TextRenderers[type].DrawString(text, pos, alignment, new Color4(color));
        }

        public static Vector2 GetTextWidth(string text, TextType type)
        {
            EnsureTypeExists(type);
           var textWid = TextRenderers[type].MeasureString(text);
            return new Vector2(textWid.Size.X,textWid.Size.Y);
        }

        public static DrawableString DrawTextContinuous(string text, TextType type, RectangleF pos, SpriteTextRenderer.TextAlignment alignment, Color color)
        {
            DrawableString d = new DrawableString(text, type, pos, alignment, color);
            textPerFrame.Add(d);
            return d;
        }
        public static void RemoveTextContinuous(DrawableString d)
        {
            textPerFrame.Remove(d);
        }

        #endregion
        #region Textures

        public static void DrawTexture(string texture, Vector2 pos, Vector2 size, double rotationAngle)
        {
            if (!DrawableImages.ContainsKey(texture))
            {
                var sdxTexture = Texture2D.FromFile(GraphicsRenderer.Device, texture);
                DrawableImages[texture] = new ShaderResourceView(GraphicsRenderer.Device, sdxTexture);
            }
            SpriteRenderer.Draw(DrawableImages[texture], pos, size, Vector2.Zero, rotationAngle, CoordinateType.Absolute);
        }

        public static DrawableTexture DrawTextureContinuous(string texture, Vector2 pos, Vector2 size, double rotationAngle)
        {
            if (!DrawableImages.ContainsKey(texture))
            {
                var sdxTexture = Texture2D.FromFile(GraphicsRenderer.Device, texture);
                DrawableImages[texture] = new ShaderResourceView(GraphicsRenderer.Device, sdxTexture);
            }
            texturesPerFrame.Add(new DrawableTexture(texture, pos, size, rotationAngle));
            return texturesPerFrame.Last();
        }

        #endregion

        public static void Update()
        {
            foreach (DrawableString str in textPerFrame)
            {
                DrawText(str.Text, str.Type, new RectangleF(0 + str.Offset.X, 0 + str.Offset.Y, GraphicsRenderer.Form.ClientSize.Width + str.Offset.Width, GraphicsRenderer.Form.ClientSize.Height + str.Offset.Height), str.Alignment, str.Color);
            }
            foreach (DrawableTexture tex in texturesPerFrame)
            {
                if (tex.Enabled)
                    DrawTexture(tex.View, tex.Position, tex.Size, tex.Rotation);
            }
        }

        private static void EnsureTypeExists(TextType t)
        {
            if (TextRenderers.ContainsKey(t)) return;

            switch (t)
            {
                case TextType.BOLD:
                    TextRenderers[t] = new TextBlockRenderer(SpriteRenderer, Constants.GlobalFont, FontWeight.Bold, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, Constants.GlobalFontSize);
                    break;
                case TextType.NORMAL:
                    TextRenderers[t] = new TextBlockRenderer(SpriteRenderer, Constants.GlobalFont, FontWeight.Normal, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, Constants.GlobalFontSize);
                    break;
                case TextType.COMIC_SANS:
                    TextRenderers[t] = new TextBlockRenderer(SpriteRenderer, "Comic Sans", FontWeight.Normal, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, Constants.GlobalFontSize);
                    break;
                case TextType.MASSIVE:
                    TextRenderers[t] = new TextBlockRenderer(SpriteRenderer, Constants.GlobalFont, FontWeight.Normal, SlimDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 50);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }
    }
}