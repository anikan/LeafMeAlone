using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Server
{
    /// <summary>
    /// Map class on the server-side, for leaf and win calculations.
    /// </summary>
    public class MapServer : GameObject
    {
        public List<MapVersion> mapVersions;

        public MapVersion activeMapVersion;

        // Width and height of the current map.
        public float Width;
        public float Height;

        /// <summary>
        /// Constructor to set up a basic map with specified width and height.
        /// </summary>
        /// <param name="width"> Width of the map. </param>
        /// <param name="height">Height of the map. </param>
        public MapServer(float width, float height)
        {
            this.Width = width;
            this.Height = height;

            // Create the tree border.
            CreateTreeBorder();

            mapVersions = new List<MapVersion>();
            mapVersions.Add(new MapVersion1());

            CreateObstacles(activeMapVersion);

        }

        public void CreateTreeBorder()
        {

            Console.WriteLine("Creating border");

            float startX = -(Width / 2.0f) + Constants.TREE_RADIUS;
            float startY = -Height / 2.0f;
            float endX = Width / 2.0f;
            float endY = Height / 2.0f;

            // Spawn trees around the border of the map!
            // Start by iterating through the height of the map, centered on origin and increase by the radius of a tree.
            for (float y = startY; y < endY; y += (Constants.TREE_RADIUS))
            {

                // Iterate through the width of the map, centered on origin and increase by radius of a tree.
                for (float x = startX; x < endX; x += (Constants.TREE_RADIUS))
                {

                    // If this is a top or bottom row, create trees.
                    if (y <= startY || endY <= (y + (Constants.TREE_RADIUS)))
                    {

                        // Set the tree's initial position.
                        Vector3 obstaclePosition = new Vector3(x, Constants.FLOOR_HEIGHT, y);

                        // If this is the bottom row.
                        if (y <= startY)
                        {
                            CreateObstacle(obstaclePosition, Constants.TREE_RADIUS * 2);
                        }
                        else
                        {
                            CreateObstacle(obstaclePosition, Constants.TREE_RADIUS);
                        }
                    }

                    // If this is the far left or right columns, create a tree.
                    else if (x <= startX || endX <= (x + (Constants.TREE_RADIUS)))
                    {

                        // Set the tree's initial position.
                        Vector3 obstaclePosition = new Vector3(x, Constants.FLOOR_HEIGHT, y);
                        CreateObstacle(obstaclePosition, Constants.TREE_RADIUS);

                    }
                }
            }
        }

        public void CreateObstacles(MapVersion version)
        {
            if (version == null)
            {
                version = mapVersions[0];
            }

            for (int i = 0; i < version.noMansObstacles.Count; i++)
            {
                Vector3 position = version.noMansObstacles[i];

                CreateObstacle(position, Constants.TREE_RADIUS);
            }

            for (int i = 0; i < version.sideObstacles.Count; i++)
            {

                Vector3 rightPosition = version.sideObstacles[i];

                Vector3 leftPosition = rightPosition;
                leftPosition.X *= -1;

                CreateObstacle(rightPosition, Constants.TREE_RADIUS);
                CreateObstacle(leftPosition, Constants.TREE_RADIUS);

            }
        }

        public void CreateObstacle(Vector3 position, float radius)
        {

            // Make a new tree.
            TreeServer newTree = new TreeServer();
            newTree.Transform.Position = position;
            newTree.Radius = radius;

            Console.WriteLine("Creating tree at " + newTree.Transform.Position);

            GameServer.instance.networkServer.SendNewObjectToAll(newTree);

        }

        /// <summary>
        /// Checks if the X/Y position (2D coordinates) are within map bounds.
        /// </summary>
        /// <param name="position">Position to check, in 2D coordinates</param>
        /// <returns>True if in bounds, false otherwise</returns>
        public bool IsInMapBounds(Vector2 position)
        {
            return IsInMapBounds(new Vector3(position.X, 0.0f, position.Y));
        }

        /// <summary>
        /// Checks if the 3D coordinate is within map bounds (only uses X and Z values).
        /// </summary>
        /// <param name="position">Position to check, in 3D coordinaates</param>
        /// <returns>True if in map bounds, false otherwise</returns>
        public bool IsInMapBounds(Vector3 position)
        {
            // Calculate bounds based on width and height of map.
            return IsInBounds(position, -Width / 2.0f, Width / 2.0f, -Height / 2.0f, Height / 2.0f);
        }

        /// <summary>
        /// Checks if a specific position is within a set of bounds.
        /// </summary>
        /// <param name="position">Position to check.</param>
        /// <param name="leftBound">Leftmost bound of the area to check.</param>
        /// <param name="rightBound">Rightmost bound of the area to check.</param>
        /// <param name="lowBound">Low (bottom) bound of the area to check</param>
        /// <param name="highBound">High (upper) bound of the area to check. </param>
        /// <returns>True if in bounds, false otherwise.</returns>
        public bool IsInBounds(Vector3 position, float leftBound, float rightBound, float lowBound, float highBound)
        {

            // Check the position against the bound values.
            return (
                 position.X >= leftBound &&
                 position.X <= rightBound &&
                 position.Z <= highBound &&
                 position.Z >= lowBound
            );
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

        public override void Update(float deltaTime)
        {
        }

        public override void Die()
        {
            // lol pls don't destroy the map
        }
    }
}
