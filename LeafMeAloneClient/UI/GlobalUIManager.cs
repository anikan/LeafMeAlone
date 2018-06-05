using System.Collections.Generic;
using System.Drawing;
using Client.UI;
using Shared;
using SlimDX;
using SpriteTextRenderer;

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
        private static UI.UI LeafBlowerTool, FlameThrowerTool;
        public static UIFindTeammate TeammateUI;


        public static List<UI.UI> UIList = new List<UI.UI>();

        /// <summary>
        /// Initialize UI Elements.
        /// </summary>
        public static void Init()
        {
            fps = new UIFramesPersecond();
            gameTimer = new UITimer(60);
            Teams = new UITeams();
            GameWinLossState = new UIGameWLState();
            Culled = new UICulled();

            if (GraphicsManager.ActivePlayer != null)
            {
                LeafBlowerTool = new UI.UI(Constants.LeafToolTip, Vector2.Zero, new Vector2(200, 50), 0);
                FlameThrowerTool = new UI.UI(Constants.FlameToolTip, Vector2.Zero, new Vector2(200, 50), 0);
                LeafBlowerTool.SetUpdateAction(() =>
                {
                    LeafBlowerTool.UITexture.Enabled = GraphicsManager.ActivePlayer.ToolEquipped == ToolType.BLOWER;
                    LeafBlowerTool.UITexture.Position = LeafBlowerTool.UITexture.Size;
                });
                FlameThrowerTool.SetUpdateAction(() =>
                {
                    FlameThrowerTool.UITexture.Enabled = GraphicsManager.ActivePlayer.ToolEquipped == ToolType.THROWER;
                    FlameThrowerTool.UITexture.Position = FlameThrowerTool.UITexture.Size;
                });

                TeammateUI = new UIFindTeammate();

                UIList.AddRange(new[] { fps, gameTimer, GameWinLossState, Culled, LeafBlowerTool,FlameThrowerTool, TeammateUI });
            }
            else
            {
                UIList.AddRange(new UI.UI[] { fps, gameTimer, GameWinLossState, Culled });
            }

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
        }
    }
}