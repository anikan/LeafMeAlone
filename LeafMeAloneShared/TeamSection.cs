using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace Shared
{
    /// <summary>
    /// One section of the map that belongs to a team.
    /// </summary>
    public class TeamSection
    {
        // Bounds of this section.
        public float leftX;
        public float rightX;
        public float upZ;
        public float downZ;

        // Number of leaves in this section.
        public int numLeaves;

        // Color of the section.
        public Vector3 sectionColor;

        // Checks if a position is in the bounds.
        public bool IsInBounds(Vector3 position)
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
                if (IsInBounds(obj.Transform.Position))
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
