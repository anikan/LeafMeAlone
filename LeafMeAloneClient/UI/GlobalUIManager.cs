using System.Drawing;
using Client.UI;

namespace Client
{
    public static class GlobalUIManager
    {

        //All the different UI.
        public static UIFramesPersecond fps;
        public static UITimer gameTimer;
        public static UITeams Teams;
        public static UIGameWLState GameWinLossState;
        public static UICulled Culled;
        public static UIFindTeammate TeammateUI;

        /// <summary>
        /// Initialize UI Elements.
        /// </summary>
        public static void Init()
        {
            fps = new UIFramesPersecond();
            gameTimer = new UITimer(60);
            Teams = new UITeams(new Size(8, 10), new Point(GraphicsRenderer.Form.ClientSize.Width / 2, 0));
            GameWinLossState = new UIGameWLState();
            Culled = new UICulled();
            //TeammateUI = new UIFindTeammate();
        }

        /// <summary>
        /// Call update for those UI which need it.
        /// </summary>
        public static void Update()
        {
            Culled.Update();
            //TeammateUI.Update();
        }
    }
}