using System.Diagnostics;
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

    public class UIThreeTwoOne
    {
        private int time = 3;
        private Stopwatch watch;
        private int audioSrc;
        private int playedSound = 0;

        public UIThreeTwoOne()
        {
            watch = new Stopwatch();
        }

        public void Start()
        {
            if ((GameClient.instance.playerClients.Count == 1 && !GraphicsRenderer.networkedCheckbox.Checked) 
                || (GameClient.instance.playerClients.Count == 4 && GraphicsRenderer.networkedCheckbox.Checked))
            {
                watch.Reset();
                watch.Start();

                int src = AudioManager.GetNewSource();
                AudioManager.SetSourceVolume(src, 0.2f);
            }
        }

        public void Update()
        {
            if ((int)watch.Elapsed.TotalSeconds >= time || watch.IsRunning == false)
            {
                watch.Stop();
                if (playedSound != 0)
                {
                    AudioManager.PlayAudio(audioSrc, Constants.CountdownGo);
                    playedSound = 0;
                }
                return;
            }
            UIManagerSpriteRenderer.DrawText((time - watch.Elapsed.Seconds).ToString(),UIManagerSpriteRenderer.TextType.SIZE300FONT,new RectangleF(0,0,Screen.Width,Screen.Height),TextAlignment.HorizontalCenter | TextAlignment.VerticalCenter,Color.AliceBlue, 200 );

            // simple countdown sequence
            for (int i = time; i >= 0; i--)
            {
                if (time - watch.Elapsed.Seconds == i && playedSound != i)
                {
                    switch (i)
                    {
                        case 3:
                            AudioManager.PlayAudio(audioSrc, Constants.CountdownThree);
                            break;
                        case 2:
                            AudioManager.PlayAudio(audioSrc, Constants.CountdownTwo);
                            break;
                        case 1:
                            AudioManager.PlayAudio(audioSrc, Constants.CountdownOne);
                            break;
                        default:
                            break;
                    }
                    playedSound = i;
                    break;
                }
            }
        }
    }
}
