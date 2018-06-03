using System.Collections.Generic;
using System.Drawing;
using Client.UI;
using Shared;
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
        public static UI.UI Tool;
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

            //Tool
            Tool = new UI.UI("Leaf Blower", UIManagerSpriteRenderer.TextType.BOLD,
               RectangleF.Empty, TextAlignment.Bottom | TextAlignment.Left, Color.White);
            Tool.SetUpdateAction(() =>
            {
                switch (GraphicsManager.ActivePlayer.ToolEquipped)
                {
                    case ToolType.SAME:
                        break;
                    case ToolType.BLOWER:
                        Tool.UIText.Text = "Leaf Blower";
                        switch (GraphicsManager.ActivePlayer.ActiveToolMode)
                        {
                            case ToolMode.PRIMARY:
                                break;
                            case ToolMode.SECONDARY:
                                Tool.UIText.Text = "Leaf Suctionery Thing";
                                break;
                        }
                        break;
                    case ToolType.THROWER:
                        Tool.UIText.Text = "Flame Thrower";
                        break;
                }
            });






            TeammateUI = new UIFindTeammate();

            UIList.AddRange(new UI.UI[] { fps, gameTimer, GameWinLossState, Culled, Tool, TeammateUI });

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