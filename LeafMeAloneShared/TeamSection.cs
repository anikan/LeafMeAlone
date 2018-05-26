using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Shared
{
    public class TeamSection
    {

        public float leftX;
        public float rightX;
        public float upZ;
        public float downZ;

        public int numLeaves;

        public Vector3 sectionColor;

        public bool IsInSquare(Vector3 position)
        {

            if (position.X > leftX && position.X < rightX && position.Z < upZ && position.Z > downZ)
            {
                return true;
            }

            return false;

        }

        public void CountObjectsInBounds(List<GameObject> positions)
        {

            numLeaves = 0;

            foreach (GameObject obj in positions)
            {
                if (IsInSquare(obj.Transform.Position))
                {
                    numLeaves++;
                //    Console.WriteLine("Num objects is now " + numLeaves);
                }
            }
        }

        public override string ToString()
        {

            return string.Format("Left Bound: {0}, Right Bound: {1}, Up Bound: {2}, Low Bound: {3}", leftX, rightX, upZ, downZ);


        }
    }
}
