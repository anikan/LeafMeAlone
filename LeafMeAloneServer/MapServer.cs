using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Server
{
    /// <summary>
    /// Map class on the server-side, for leaf and win calculations.
    /// </summary>
    public class MapServer : Map
    {
        public MapServer(float width, float height) : base(width, height)
        {
        }

        /// <summary>
        /// Gets all of the leaves that are within specified bounds.
        /// </summary>
        /// <param name="leaves"> List of all leaves in the game. </param>
        /// <param name="leftBound"> Leftmost bound of the area to check. </param>
        /// <param name="rightBound"> Rightmost bound of the area to check. </param>
        /// <param name="lowBound"> Low (bottom) bound of the area to check. </param>
        /// <param name="highBound"> High (upper) bound of the area to check. </param>
        /// <returns>Number of leaves in the area.</returns>
        public int GetLeavesInBounds(List<LeafServer> leaves, float leftBound, float rightBound, float lowBound, float highBound)
        {
            // Keep track of number of leaves.
            int count = 0;

            // Iterate through all leaves.
            for (int i = 0; i < leaves.Count; i++)
            {
                // Current leaf.
                LeafServer leaf = leaves[i];

                // Check if it's in bounds.
                if (IsInBounds(leaf.Transform.Position, leftBound, rightBound, lowBound, highBound))
                {
                    // If so, increment count.
                    count++;
                }
            }

            // Return result.
            return count;

        }
    }
}
