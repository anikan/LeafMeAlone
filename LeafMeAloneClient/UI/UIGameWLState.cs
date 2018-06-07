using System;
using System.Drawing;
using Shared;
using SpriteTextRenderer;
using System.IO;

namespace Client.UI
{
    public class UIGameWLState : UI
    {
        public UI LeafStatsUI, ShameStatsUI, PlayerStatsUI;

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
            new RectangleF(0,70,0,0), TextAlignment.HorizontalCenter | TextAlignment.Top, Color.Transparent)
        {
            SetState(WinLoseState.None);
            LeafStatsUI = new UI("", UIManagerSpriteRenderer.TextType.SMALL, new RectangleF(25, 0, 0, 0),
                TextAlignment.Left | TextAlignment.VerticalCenter, Color.White);
            PlayerStatsUI = new UI("", UIManagerSpriteRenderer.TextType.SMALL, new RectangleF(0, 0, 0, 0),
                TextAlignment.VerticalCenter | TextAlignment.HorizontalCenter, Color.White);
            ShameStatsUI = new UI("", UIManagerSpriteRenderer.TextType.SMALL, new RectangleF(0, 0, -25, 0),
                TextAlignment.Right | TextAlignment.VerticalCenter, Color.White);
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

        public void SetStats(PlayerStats st)
        {
            if (st == null)
            {
                LeafStatsUI.UIText.Text = PlayerStatsUI.UIText.Text = ShameStatsUI.UIText.Text = "";
            }
            else
            {
                LeafStatsUI.UIText.Text = st.ToString(PlayerStats.PlayerStatsEnum.LeafStats);
                PlayerStatsUI.UIText.Text = st.ToString(PlayerStats.PlayerStatsEnum.PlayerStats);
                ShameStatsUI.UIText.Text = st.ToString(PlayerStats.PlayerStatsEnum.ShameStats);

            }
        }

        public override void Update()
        {
            
        }

    }
}
