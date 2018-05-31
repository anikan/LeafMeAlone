using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client.UI
{
    class UIHealth
    {

        private GameObject FollowGameobject;
        private DrawableTexture HealthBar;

        public UIHealth(GameObject followGameObject)
        {
            FollowGameobject = followGameObject;
            HealthBar = UIManagerSpriteRenderer.DrawTextureContinuous(Constants.Arrow, new Vector2(100, 100),
                new Vector2(75, 75), 0);
            Vector2 pos = GraphicsManager.WorldToScreenPoint(FollowGameobject.Transform.Position);
            pos.X -= HealthBar.Size.X / 2f;
            pos.Y -= HealthBar.Size.Y;
            HealthBar.Position = pos;
        }

    }
}
