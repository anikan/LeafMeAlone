using System.Drawing;
using Shared;
using SlimDX;

namespace Client.UI
{
    public class UINickname
    {
        private readonly GameObject FollowGameObject;

        public bool enabled = true;
        private readonly float delta = 100f;
        public UINickname(GameObject followGameObject, string nickname)
        {
            FollowGameObject = followGameObject;
        }

        public void Update()
        {
            if (!enabled)
                return;

            var p = GraphicsManager.WorldToScreenPoint(FollowGameObject.Transform.Position) - new Vector2(30,150);
            UIManagerSpriteRenderer.EnsureTypeExists(UIManagerSpriteRenderer.TextType.NORMAL);
            UIManagerSpriteRenderer.TextRenderers[UIManagerSpriteRenderer.TextType.NORMAL]
                .DrawString(FollowGameObject.Name, p, new Color4(Color.AliceBlue));
        }

    }
}