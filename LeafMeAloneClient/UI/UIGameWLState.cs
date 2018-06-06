using System;
using System.Drawing;
using Shared;
using SpriteTextRenderer;

namespace Client.UI
{
    public class UIGameWLState : UI
    {
        public UI StatsUI;


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
            RectangleF.Empty, TextAlignment.HorizontalCenter | TextAlignment.Top, Color.Transparent)
        {
            SetState(WinLoseState.None);
            StatsUI = new UI("", UIManagerSpriteRenderer.TextType.SMALL, new RectangleF(0,0,0,70), 
                TextAlignment.Top | TextAlignment.HorizontalCenter, Color.White);
        }


        /// <summary>
        /// Set whether the player has won, lost, or not yet.
        /// </summary>
        /// <param name="s"></param>
        public void SetState(WinLoseState s)
        {
            PlayerStats st = new PlayerStats();
            currWinLoseState = s;
            switch (currWinLoseState)
            {
                case WinLoseState.Win:
                    UIText.Text = Constants.WinText;
                    UIText.Color = Color.Green;
                    StatsUI.UIText.Text = st.ToString();
                    break;
                case WinLoseState.Lose:
                    UIText.Text = Constants.LoseText;
                    UIText.Color = Color.Red;
                    StatsUI.UIText.Text = st.ToString();
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
