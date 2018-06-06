using System;
using Shared;
using SlimDX;

namespace Client.UI
{
    public class UIFindTeammate : UI
    {
        public PlayerClient Teammate;

        //What proportion of the screen the indicator should take up.
        float screenProportion = 1/7.0f;

        //Image proportion. Optimally would be calculated automatically.
        private float imageProportion = 435.0f/ 358.0f;

        public UIFindTeammate() : base(Constants.TeammateIndicator, new Vector2(100, 100),
            new Vector2(75, 75), 0)
        {
            foreach (PlayerClient client in GameClient.instance.playerClients)
            {
                if (client != GraphicsManager.ActivePlayer && client.PlayerTeam == GraphicsManager.ActivePlayer.PlayerTeam)
                {
                    Teammate = client;
                    break;
                }
            }
            UITexture.Enabled = false;
        }

        Vector2 IntersectScreenEdge(Vector2 vectorToTeammate)
        {
            Vector2[] screenpts = { Vector2.UnitX * Screen.Width, Vector2.UnitY * Screen.Height };


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
                    if (client != GraphicsManager.ActivePlayer && client.PlayerTeam == GraphicsManager.ActivePlayer.PlayerTeam)
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
                    
                    //Really should only recalculate size when window changes.
                    UITexture.Size = new Vector2(Screen.Height * screenProportion * imageProportion, Screen.Height * screenProportion);
                    
                    //Find where teammate is in screenspace.
                    Vector2 teammatePos = GraphicsManager.WorldToScreenPoint(Teammate.Transform.Position);
                    
                    //Place the UI Texture where the teammate would be, then clamp it back in.
                    UITexture.Position = Vector2.Clamp(teammatePos, UITexture.Size/2.0f, new Vector2(Screen.Width, Screen.Height) - UITexture.Size / 2.0f);

                    //Offset image by size
                    UITexture.Position -= UITexture.Size / 2;
                }
                else
                {
                    UITexture.Enabled = false;
                }
            }
        }

    }
}
