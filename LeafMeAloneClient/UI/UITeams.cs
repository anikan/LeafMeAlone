using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntTweakBar;
using Shared;

namespace Client.UI
{
    public class UITeams
    {
        public IntVariable Team1_Leaves, Team2_Leaves;
        public VectorVariable TeammateDirectionVar;
        private GameObject followedGameObject;
        private GameObject playerGameObject;

        public UITeams(Size size, Point location)
        {


            Size size_new = new Size(size.Width / 2, size.Height / 2);
            Console.WriteLine(size_new);
            Team1_Leaves = new IntVariable(UIManagerAntTweakBar.Create("Team1", size_new,new Point(location.X - 120, location.Y)))
            {
                ReadOnly = true,
                Label = " ",
                Value = 0
            };
            UIManagerAntTweakBar.ActiveUI["Team1"].Color = Color.Red;


            Team2_Leaves = new IntVariable(UIManagerAntTweakBar.Create("Team2", size_new, new Point(location.X, location.Y)))
            {
                ReadOnly = true,
                Label = " ",
                Value = 0
            };
            UIManagerAntTweakBar.ActiveUI["Team2"].Color = Color.Blue;

            
            
        }

        public void Update()
        {
            if (followedGameObject == null) return;

            TeammateDirectionVar.X =
                followedGameObject.Transform.Position.X - playerGameObject.Transform.Position.X;
            TeammateDirectionVar.Y =
                followedGameObject.Transform.Position.Y - playerGameObject.Transform.Position.Y;
            TeammateDirectionVar.Z =
                followedGameObject.Transform.Position.Z - playerGameObject.Transform.Position.Z;
        }

        
        public void SetFollow(GameObject player, GameObject obj)
        {
            if(TeammateDirectionVar == null)
            {
                TeammateDirectionVar =
                    new VectorVariable(UIManagerAntTweakBar.Create("Team_" + obj.Id, new Size(250, 250), new Point(50, 50)))
                    {
                        Label = " ",
                        ShowValue = false
                    };
            }
            followedGameObject = obj;
            playerGameObject = player;
        }


    }
}
