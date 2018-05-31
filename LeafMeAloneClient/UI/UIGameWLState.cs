using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntTweakBar;
using Shared;
using SlimDX;
using SpriteTextRenderer;

namespace Client.UI
{
    public enum State
    {
        Win,
        Lose,
        None
    }

    public class UIGameWLState
    {
        private State CurrState = State.Win;

        public DrawableString StateText;
        public UIGameWLState()
        {
            StateText = UIManagerSpriteRenderer.DrawTextContinuous("", UIManagerSpriteRenderer.TextType.MASSIVE, 
                new RectangleF(0, 0, GraphicsRenderer.Form.Width, GraphicsRenderer.Form.Height), TextAlignment.HorizontalCenter | TextAlignment.VerticalCenter, Color.Transparent);
            SetState(State.None);
        }

        public void SetState(State s)
        {
            CurrState = s;
            switch (CurrState)
            {
                case State.Win:
                    StateText.Text = Constants.WinText;
                    StateText.Color = Color.Green;
                    break;
                case State.Lose:
                    StateText.Text = Constants.LoseText;
                    StateText.Color = Color.Red;
                    break;
                case State.None:
                    StateText.Text = String.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }

        }


    }
}
