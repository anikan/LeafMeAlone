using Shared;
using SpriteTextRenderer;
using System.Drawing;

namespace Client.UI
{
    public class UITeams
    {

        //UI for team leaves.
        public UI Team1_Leaves, Team2_Leaves;

        public UITeams()
        {
            Team1_Leaves = new UI("0", UIManagerSpriteRenderer.TextType.BOLD, new RectangleF(-25, Constants.GlobalFontSize,0 , Constants.GlobalFontSize*2), TextAlignment.Top | TextAlignment.HorizontalCenter, Color.LightCoral);
            Team2_Leaves = new UI("0", UIManagerSpriteRenderer.TextType.BOLD, new RectangleF(0, Constants.GlobalFontSize, 25, Constants.GlobalFontSize*2), TextAlignment.Top | TextAlignment.HorizontalCenter, Color.LightBlue);
        }
    }
}
