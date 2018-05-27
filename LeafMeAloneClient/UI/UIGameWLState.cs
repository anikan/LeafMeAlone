using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntTweakBar;
using Shared;

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
        private State CurrState = State.None;

        public StringVariable StateText;
        public UIGameWLState(Size size, Point location)
        {
            StateText = new StringVariable(UIManager.Create(" ",size,location))
            {
                ReadOnly = true,
                Label = " "
            };
            SetState(State.Win);
        }

        public void SetState(State s)
        {
            CurrState = s;
            switch (s)
            {
                case State.Win:
                    StateText.Value = Constants.WinText;
                    UIManager.ActiveUI[" "].Color = Color.Green;
                    UIManager.ActiveUI[" "].Visible = true;
                    break;
                case State.Lose:
                    StateText.Value = Constants.LoseText;
                    UIManager.ActiveUI[" "].Color = Color.Red;
                    UIManager.ActiveUI[" "].Visible = true;
                    StateText.Visible = true;
                    break;
                case State.None:
                    StateText.Value = String.Empty;
                    StateText.Visible = false;
                    UIManager.ActiveUI[" "].Visible = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }

        }


    }
}
