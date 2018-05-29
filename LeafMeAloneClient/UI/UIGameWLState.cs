using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntTweakBar;
using Shared;
using SlimDX;

namespace Client.UI
{
    public enum State
    {
        Win,
        Lose,
        None
    }
    class UIGameWLState
    {
        private State CurrState = State.Win;

        public DrawableString StateText;
        public UIGameWLState(Vector2 position)
        {
            StateText = UIManager2.DrawTextContinuous("", UIManager2.TextType.MASSIVE, position, Color.Transparent);
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
