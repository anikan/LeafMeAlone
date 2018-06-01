using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client.UI
{
    public class UIFindTeammate
    {
        public PlayerClient Teammate;
        private readonly DrawableTexture teammateFinder;

        public UIFindTeammate()
        {
            foreach (PlayerClient client in GameClient.instance.playerClients)
            {
                if (client != GraphicsManager.ActivePlayer && client.team == GraphicsManager.ActivePlayer.team)
                {
                    Teammate = client;
                    break;
                }
            }
            teammateFinder = UIManagerSpriteRenderer.DrawTextureContinuous(Constants.Arrow, new Vector2(100, 100),
                 new Vector2(75, 75), 0);
            teammateFinder.Enabled = false;
        }

        Vector2 IntersectScreenEdge(Vector2 vectorToTeammate)
        {
            Vector2[] screenpts = { Vector2.UnitX * GraphicsRenderer.Form.ClientSize.Width, Vector2.UnitY * GraphicsRenderer.Form.ClientSize.Height };


            Vector2 intersection = Vector2.Zero;
            foreach (Vector2 pt in screenpts)
            {
                intersection = vectorToTeammate.IntersectionPoint(pt);
                if (float.IsNaN(intersection.X) || float.IsNaN(intersection.Y) || float.IsInfinity(intersection.Y) ||
                    float.IsInfinity(intersection.X))
                    intersection = Vector2.Zero;
                else
                    return intersection;

                intersection = -vectorToTeammate.IntersectionPoint(pt);
                if (float.IsNaN(intersection.X) || float.IsNaN(intersection.Y) || float.IsInfinity(intersection.Y) ||
                    float.IsInfinity(intersection.X))
                    intersection = Vector2.Zero;
                else
                    return intersection;

            }
            return intersection;
        }

        public void Update()
        {
            if (Teammate == null)
            {
                foreach (PlayerClient client in GameClient.instance.playerClients)
                {
                    if (client != GraphicsManager.ActivePlayer && client.team == GraphicsManager.ActivePlayer.team)
                    {
                        Teammate = client;
                        break;
                    }
                }
            }
            if (Teammate != null)
            {
                if (Teammate.model.IsCulled)
                {
                    teammateFinder.Enabled = true;

                    Vector2 teammatePos = GraphicsManager.WorldToScreenPoint(Teammate.Transform.Position);
                    Vector2 myPos = GraphicsManager.WorldToScreenPoint(GraphicsManager.ActivePlayer.Transform.Position);

                    Vector2 vectortoTeammate = myPos - teammatePos;
                    Vector2 newPos = IntersectScreenEdge(vectortoTeammate);

                    newPos.X -= teammateFinder.Size.X;
                    newPos.Y -= teammateFinder.Size.Y;
                    teammateFinder.Position = newPos;
                    Debug.Log(newPos.ToString());
                }
                else
                {
                    teammateFinder.Enabled = false;
                }
            }
        }

    }
}
