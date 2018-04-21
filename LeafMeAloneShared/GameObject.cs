using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public abstract class GameObject
    {
        private int id;

        protected Transform Transform;

        public abstract void Update();
        public abstract void Draw();

        public int GetId()
        {
            return id;
        }
    }
}
