using System.Drawing;
using Shared;
using SlimDX;
using SpriteTextRenderer;

namespace Client.UI
{
    public class UITeams
    {

        //UI for team leaves.
        public UI Team1_Leaves, Team2_Leaves;

        public UITeams()
        {
            Team1_Leaves = new UI("0", UIManagerSpriteRenderer.TextType.NORMAL, new RectangleF(0, 0, -100, -100), TextAlignment.Top | TextAlignment.HorizontalCenter, Color.Red);
            Team2_Leaves = new UI("0", UIManagerSpriteRenderer.TextType.NORMAL, new RectangleF(0, 0, 0, 0), TextAlignment.Top | TextAlignment.HorizontalCenter, Color.Blue);
        }
    }
}
