using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Client
{
    public abstract class GameObjectClient : GameObject
    {
        private Model model;

        public void SetModel(string filePath)
        {
            model = new Model(filePath);
            Name = filePath.Split('.')[0];
        }

        public override void Update()
        {
            if (model == null)
                return;
            model.m_Properties = Transform;
            model.Update();
        }

        public override void Draw()
        {
            model?.Draw();
        }

        public abstract void UpdateFromPacket(Packet packet);
    }
}
