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

        public List<Vector3> spawnPoints;
        // Color of the section.
        public Vector3 sectionColor;
        internal Team team;

        public TeamSection()
        {
            spawnPoints = new List<Vector3>()
            {
                new Vector3(leftX + new Random().Range(Math.Abs(rightX - leftX)), 0, 
                            downZ + new Random().Range(Math.Abs(upZ - downZ))),
                new Vector3(leftX + new Random().Range(Math.Abs(rightX - leftX)), 0, 
                            downZ + new Random().Range(Math.Abs(upZ - downZ))),
                new Vector3(leftX + new Random().Range(Math.Abs(rightX - leftX)), 0, 
                            downZ + new Random().Range(Math.Abs(upZ - downZ))),
                new Vector3(leftX + new Random().Range(Math.Abs(rightX - leftX)), 0, 
                            downZ + new Random().Range(Math.Abs(upZ - downZ)))
            };
        }

        // Checks if a position is in the bounds.
        public bool IsInBounds(Vector3 position)
        {

            // Is it in bounds? If so, return true.
            if (position.X >= leftX && position.X <= rightX && position.Z <= upZ && position.Z >= downZ)
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// Count all of the objects against all of the different sections, and update values.
        /// </summary>
        /// <param name="positions">Objects to check.</param>
        public void CountObjectsInBounds(List<GameObject> positions)
        {

            // Set the number of leaves in this section to zero.
            numLeaves = 0;

            // Iterate through all the passed in object positions.
            foreach (GameObject obj in positions)
            {
                // Check if the position is in bounds.
                if (IsInBounds(obj.Transform.Position))
                {
                    // Increase the number of leaves.
                    numLeaves++;

                //    Console.WriteLine("Num objects is now " + numLeaves);
                }
            }
        }

        /// <summary>
        /// Get this section as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            // Just print out the bounds.
            return string.Format("Left Bound: {0}, Right Bound: {1}, Up Bound: {2}, Low Bound: {3}", leftX, rightX, upZ, downZ);

        }

        /// <summary>
        /// Gets a new random spawn point for the player.
        /// </summary>
        /// <returns>A vector 3 of the spawn point</returns>
        public Vector3 GetRandomSpawnPoint()
        {
            int index = new Random().Next(spawnPoints.Count);
            return spawnPoints.ElementAt(index);
        }

        /// <summary>
        /// Gets the next spawn point of the player spawn index
        /// </summary>
        /// <returns>The vector 3 of the next spawn point</returns>
        public Vector3 NextSpawnPoint(int index)
        {
            return spawnPoints[(index % spawnPoints.Count)];
        }
    }
}
