using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client.UI
{
    public class UIHealth
    {
        private GameObject FollowGameobject;
        public DrawableTexture HealthBar;

        private float delta = 100f;
        public UIHealth(GameObject followGameObject, Team team)
        {
            FollowGameobject = followGameObject;
            HealthBar = UIManagerSpriteRenderer.DrawTextureContinuous(team == Team.RED ? Constants.RedHealth : Constants.BlueHealth, new Vector2(100, 100),
                new Vector2(50, 10), 0);

            HealthBar.Size.X = 50 * (FollowGameobject.Health / Constants.PLAYER_HEALTH);
            HealthBar.Size.Y = 10;
            HealthBar.Position = GraphicsManager.WorldToScreenPoint(FollowGameobject.Transform.Position) - new Vector2(HealthBar.Size.X / 2f, HealthBar.Size.Y + delta);
        }

        public void Update()
        {
            HealthBar.Size.X = 50 * (FollowGameobject.Health / Constants.PLAYER_HEALTH );
            HealthBar.Size.Y = 10;

            var alignHealthBar = new Vector2((((1.0f - (FollowGameobject.Health / Constants.PLAYER_HEALTH)) / 2f) * 50), 0);


            HealthBar.Position = GraphicsManager.WorldToScreenPoint(FollowGameobject.Transform.Position) - new Vector2(HealthBar.Size.X/2f, HealthBar.Size.Y + delta) - alignHealthBar;
        }

    }
}
