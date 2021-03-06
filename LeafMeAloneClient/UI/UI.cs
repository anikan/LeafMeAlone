﻿using System;
using System.Drawing;
using SlimDX;

namespace Client.UI
{
    public class UI
    {
        public DrawableString UIText;
        public DrawableTexture UITexture;

        public Action UpdateAction;

        /// <summary>
        /// Text UI.
        /// </summary>
        public UI(string text, UIManagerSpriteRenderer.TextType type, RectangleF offset, SpriteTextRenderer.TextAlignment alignment, Color color)
        {
            UIText = UIManagerSpriteRenderer.DrawTextContinuous(text, type, offset, alignment, color);
        }

        public void SetUpdateAction(Action a)
        {
            UpdateAction = a;
        }

        /// <summary>
        /// Texture UI.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="rotationAngle"></param>
        public UI(string texture, Vector2 pos, Vector2 size, double rotationAngle)
        {
            UITexture = UIManagerSpriteRenderer.DrawTextureContinuous(texture, pos, size, rotationAngle);
        }


        public virtual void Update()
        {
            UpdateAction?.Invoke();
        }


    }
}
