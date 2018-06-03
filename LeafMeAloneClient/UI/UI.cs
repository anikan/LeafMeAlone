using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public UI(string text, UIManagerSpriteRenderer.TextType type, RectangleF pos, SpriteTextRenderer.TextAlignment alignment, Color color)
        {
            UIText = UIManagerSpriteRenderer.DrawTextContinuous(text, type, pos, alignment, color);
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
        public UI(string texture, UIManagerSpriteRenderer.TextType bOLD, Vector2 pos, Vector2 size, double rotationAngle)
        {
            UITexture = UIManagerSpriteRenderer.DrawTextureContinuous(texture, pos, size, rotationAngle);
        }


        public virtual void Update()
        {
            UpdateAction?.Invoke();
        }


    }
}
