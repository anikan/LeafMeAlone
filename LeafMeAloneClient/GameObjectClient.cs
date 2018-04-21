using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    class GameObjectClient : GameObject
    {
        private Model model;

        public void SetModel(string filePath)
        {
            model = new Model(filePath);
        }

        public override void Update()
        {
            if (model == null)
                return;
            model.M = Transform;
            model.Update();
        }

        public override void Draw()
        {
            model?.Draw();
        }
    }
}
