using System.Collections.Generic;
using Shared;
using SlimDX;

namespace Client.UI
{
    public static class GlobalUIManager
    {

        //All the different UI.
        public static UIFramesPersecond fps;
        public static UITimer gameTimer;
        public static UITeams Teams;
        public static UIGameWLState GameWinLossState;
        private static Client.UI.UI LeafBlowerTool, FlameThrowerTool;
        public static UIFindTeammate TeammateUI;
        public static UIThreeTwoOne Countdown;


        public static List<Client.UI.UI> UIList = new List<Client.UI.UI>();

        /// <summary>
        /// Initialize UI Elements.
        /// </summary>
        public static void Init()
        {
            TeammateUI = new UIFindTeammate();

            fps = new UIFramesPersecond();
            gameTimer = new UITimer(60);
            Teams = new UITeams();
            GameWinLossState = new UIGameWLState();
            LeafBlowerTool = new UI(Constants.LeafToolTip, Vector2.Zero, new Vector2(0,0), 0);
            FlameThrowerTool = new UI(Constants.FlameToolTip, Vector2.Zero, new Vector2(0,0), 0);
            Countdown = new UIThreeTwoOne();

            const float takeUp = 1f / 8f;
            const float sizeportion = 1649f / 375f;
            LeafBlowerTool.SetUpdateAction(() =>
                {
                    float xSize = Screen.Height * takeUp * sizeportion;
                    float ySize = Screen.Height * takeUp;
                    if (GraphicsManager.ActivePlayer != null)
                    {
                        LeafBlowerTool.UITexture.Enabled = GraphicsManager.ActivePlayer.ToolEquipped == ToolType.BLOWER;
                        LeafBlowerTool.UITexture.Position = new Vector2(0,Screen.Height - ySize);
                        LeafBlowerTool.UITexture.Size = new Vector2(xSize, ySize);
                    }
                });
            FlameThrowerTool.SetUpdateAction(() =>
                {
                    float xSize = Screen.Height * takeUp * sizeportion;
                    float ySize = Screen.Height * takeUp;
                    if (GraphicsManager.ActivePlayer != null)
                    {
                        FlameThrowerTool.UITexture.Enabled = GraphicsManager.ActivePlayer.ToolEquipped == ToolType.THROWER;
                        FlameThrowerTool.UITexture.Position = new Vector2(0, Screen.Height - ySize);
                        FlameThrowerTool.UITexture.Size = new Vector2(xSize,ySize);
                    }
                });

                UIList.AddRange(new[] { GameWinLossState,TeammateUI, fps, gameTimer, LeafBlowerTool,FlameThrowerTool });

        }

        /// <summary>
        /// Call update for those UI which need it.
        /// </summary>
        public static void Update()
        {
            foreach (var ui in UIList)
            {
                ui.Update();
            }
            Countdown.Update();
        }
    }
}