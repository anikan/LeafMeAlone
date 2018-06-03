using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client.UI
{
    public class UIFindTeammate : UI
    {
        public PlayerClient Teammate;

        public UIFindTeammate() : base(Constants.Arrow, new Vector2(100, 100),
            new Vector2(75, 75), 0)
        {
            foreach (PlayerClient client in GameClient.instance.playerClients)
            {
                if (client != GraphicsManager.ActivePlayer && client.team == GraphicsManager.ActivePlayer.team)
                {
                    Teammate = client;
                    break;
                }
            }
            UITexture.Enabled = false;
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

        public override void Update()
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
                    UITexture.Enabled = true;
                    //Vector2 teammatePos = GraphicsManager.WorldToScreenPoint(Teammate.Transform.Position);
                    //Vector2 myPos = GraphicsManager.WorldToScreenPoint(GraphicsManager.ActivePlayer.Transform.Position);
                    //var direction = myPos - teammatePos;



                    //teammatePos -= new Vector2(.5f, .5f);
                    //teammatePos *= 2;
                    //var max = Math.Max(Math.Abs(teammatePos.X), Math.Abs(teammatePos.Y));
                    //teammatePos = (teammatePos / (max * 2.0f)) + new Vector2(.5f, .5f);

                    //teammatePos.X *= GraphicsRenderer.Form.ClientSize.Width;
                    //teammatePos.Y *= GraphicsRenderer.Form.ClientSize.Height;

                    //teammatePos.X -= teammateFinder.Size.X;
                    //teammatePos.Y -= teammateFinder.Size.Y;

                    //teammateFinder.Position = teammatePos;
                    //Debug.Log(teammatePos.ToString());

                    var range = new Vector2(GraphicsRenderer.Form.ClientSize.Width/6f, GraphicsRenderer.Form.ClientSize.Height/6f);

                    var teammatePos = Teammate.Transform.Position;
                    var myPos = GraphicsManager.ActivePlayer.Transform.Position;
                    var direction = teammatePos - myPos;
                    direction.Normalize();
                    direction.Y -= .1f;

                    var indPos = new Vector2(range.X * direction.X,range.Y *direction.Y);
                    indPos = new Vector2(indPos.X + GraphicsRenderer.Form.ClientSize.Width/2f, indPos.Y + GraphicsRenderer.Form.ClientSize.Height / 2f);

                    //var pdir = myPos - GraphicsManager.ScreenToWorldPoint(new Vector2(indPos.X, indPos.Y));
                   // pdir.Normalize();

                    //float angle = ((float)Math.Atan2(pdir.X, pdir.Y));

                    UITexture.Position = indPos;
                    Debug.Log(indPos.ToString());

                    

                    //Vector2 teammatePos = GraphicsManager.WorldToScreenPoint(Teammate.Transform.Position);
                    //Vector2 myPos = GraphicsManager.WorldToScreenPoint(GraphicsManager.ActivePlayer.Transform.Position);

                    //Vector2 vectortoTeammate = myPos - teammatePos;
                    //Vector2 newPos = IntersectScreenEdge(vectortoTeammate);

                    //newPos.X -= teammateFinder.Size.X;
                    //newPos.Y -= teammateFinder.Size.Y;
                    //teammateFinder.Position = newPos;
                    //Debug.Log(newPos.ToString());
                }
                else
                {
                    UITexture.Enabled = false;
                }
            }
        }

    }
}
