using System.Drawing;
using Client.UI;

namespace Client
{
    public static class GlobalUIManager
    {
        public static UIFramesPersecond fps;
        public static UITimer gameTimer;
        public static UITeams Teams;
        public static UIGameWLState GameWinLossState;
        public static UICulled Culled;
        public static UIFindTeammate TeammateUI;


        public static void Init()
        {
            fps = new UIFramesPersecond();
            gameTimer = new UITimer(60);
            Teams = new UITeams(new Size(8, 10), new Point(GraphicsRenderer.Form.ClientSize.Width / 2, 0));
            GameWinLossState = new UIGameWLState();
            Culled = new UICulled();
            TeammateUI = new UIFindTeammate();
        }

        public static void Update()
        {
            Culled.Update();
            TeammateUI.Update();
        }
    }
}