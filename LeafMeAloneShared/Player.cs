using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    class Player
    {
        public const float SPEED = 1.0f;

        private Vector3 CurrentPos;

        public enum MoveDirection
        {
            NORTH,
            EAST,
            SOUTH,
            WEST
        };

        public void Move(MoveDirection dir)
        {
            Vector3 delta = Vector3.Zero;
            switch (dir)
            {
                case MoveDirection.NORTH:
                    delta = new Vector3(0, 1, 0);
                    break;
                case MoveDirection.EAST:
                    delta = new Vector3(1, 0, 0);
                    break;
                case MoveDirection.SOUTH:
                    delta = new Vector3(0, -1, 0);
                    break;
                case MoveDirection.WEST:
                    delta = new Vector3(-1, 0, 0);
                    break;
            } 
        }
    }
}
