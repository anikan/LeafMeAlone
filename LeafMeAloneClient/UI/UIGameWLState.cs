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
    public class UIGameWLState : UI
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
        
        public UIGameWLState() : base("", UIManagerSpriteRenderer.TextType.MASSIVE,
            RectangleF.Empty, TextAlignment.HorizontalCenter | TextAlignment.VerticalCenter, Color.Transparent)
        {
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
                    UIText.Text = Constants.WinText;
                    UIText.Color = Color.Green;
                    break;
                case WinLoseState.Lose:
                    UIText.Text = Constants.LoseText;
                    UIText.Color = Color.Red;
                    break;
                case WinLoseState.None:
                    UIText.Text = String.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }

        }


        public override void Update()
        {
            
        }
    }
}
