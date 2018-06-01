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
    public class UIGameWLState
    {
        /// <summary>
        /// Winning or losing state.
        /// </summary>
        public enum WinLoseState
        {
            None,
            Win,
            Lose
        }

        /// <summary>
        /// Win or Lose or None.
        /// </summary>
        private WinLoseState currWinLoseState = WinLoseState.None;

        public DrawableString StateText;
        public UIGameWLState()
        {
            StateText = UIManagerSpriteRenderer.DrawTextContinuous("", UIManagerSpriteRenderer.TextType.MASSIVE, 
                new RectangleF(0, 0, GraphicsRenderer.Form.Width, GraphicsRenderer.Form.Height), TextAlignment.HorizontalCenter | TextAlignment.VerticalCenter, Color.Transparent);
            SetState(WinLoseState.None);
        }


        /// <summary>
        /// Set whether the player has won, lost, or not yet.
        /// </summary>
        /// <param name="s"></param>
        public void SetState(WinLoseState s)
        {
            currWinLoseState = s;
            switch (currWinLoseState)
            {
                case WinLoseState.Win:
                    StateText.Text = Constants.WinText;
                    StateText.Color = Color.Green;
                    break;
                case WinLoseState.Lose:
                    StateText.Text = Constants.LoseText;
                    StateText.Color = Color.Red;
                    break;
                case WinLoseState.None:
                    StateText.Text = String.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }

        }


    }
}
