using System.Drawing;
using Shared;
using SlimDX;
using SpriteTextRenderer;

namespace Client.UI
{
    public class UIHealth : UI
    {
        private readonly GameObject FollowGameObject;

        private readonly float delta = 100f;
        public UIHealth(GameObject followGameObject, TeamName team) : base(team == TeamName.RED ? Constants.RedHealth : Constants.BlueHealth, new Vector2(100, 100),
            new Vector2(50, 10), 0)
        {
            FollowGameObject = followGameObject;
            UITexture.Size.X = 50 * (FollowGameObject.Health / Constants.PLAYER_HEALTH);
            UITexture.Size.Y = 10;
            UITexture.Position = GraphicsManager.WorldToScreenPoint(FollowGameObject.Transform.Position) - new Vector2(UITexture.Size.X / 2f, UITexture.Size.Y + delta);
        }

        public override void Update()
        {
            UITexture.Size.X = 50 * (FollowGameObject.Health / Constants.PLAYER_HEALTH);
            UITexture.Size.Y = 10;

            var alignHealthBar = new Vector2((((1.0f - (FollowGameObject.Health / Constants.PLAYER_HEALTH)) / 2f) * 50), 0);


            UITexture.Position = GraphicsManager.WorldToScreenPoint(FollowGameObject.Transform.Position) - new Vector2(UITexture.Size.X / 2f, UITexture.Size.Y + delta) - alignHealthBar;
        }

    }
    public class UINickname
    {
        private readonly GameObject FollowGameObject;

        private readonly float delta = 100f;
        public UINickname(GameObject followGameObject, string nickname)
        {
            FollowGameObject = followGameObject;
        }

        public void Update()
        {
            var p = GraphicsManager.WorldToScreenPoint(FollowGameObject.Transform.Position) - new Vector2(30,150);
            UIManagerSpriteRenderer.EnsureTypeExists(UIManagerSpriteRenderer.TextType.NORMAL);
            UIManagerSpriteRenderer.TextRenderers[UIManagerSpriteRenderer.TextType.NORMAL]
                .DrawString(FollowGameObject.Name, p, new Color4(Color.AliceBlue));
        }

    }
}
