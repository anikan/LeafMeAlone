using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using SlimDX;

namespace Client
{
    /// <summary>
    /// Client-side game map. Mainly for just rendering a terrain.
    /// </summary>
    public class MapClient : NonNetworkedGameObjectClient
    {

        private Random rnd;

        // Number of tiles beyond the treeline (so player can't see skybox).

        // Number of tiles that make up the width and height of the map, not including border tiles.
        private const int NUM_TILES_WIDTH = (int)(Constants.MAP_WIDTH / Constants.TILE_SIZE);
        private const int NUM_TILES_HEIGHT = (int)(Constants.MAP_HEIGHT / Constants.TILE_SIZE);

        private static int NUM_BORDER_TILES = (int)Math.Ceiling(Constants.OUTER_BORDER_SIZE / Constants.TILE_SIZE);

        // List of all map tiles in the game.
        private List<MapTile> AllMapTiles;

        private List<List<MapTile>> TeamTiles;

        // Active match information.
        private Match activeMatch;

        /// <summary>
        /// Creates a new map with a map model.
        /// </summary>
        /// <param name="modelPath"></param>
        public MapClient() : base()
        {

            // Random number generator, to be used for y offsets. 
            rnd = new Random();

            // Just use the default match for now.
            activeMatch = Match.DefaultMatch;


            // Create the map tiles.
            CreateMapTiles();
       
            // Assign tiles to sections of the map
            CreateDistinctTeamSections(activeMatch);

        }

        /// <summary>
        /// Create all the tiles on the map.
        /// </summary>
        public void CreateMapTiles()
        {

            // List of all map tiles.
            AllMapTiles = new List<MapTile>();


            // Start in the middle, to make sure all sides are even.
            float y = 0.0f;

            // Calculate number of tiles on each side of middle column.
            int tilesOnEachSide = (NUM_TILES_HEIGHT / 2) - 1 + NUM_BORDER_TILES;

            // Create the middle column.
            CreateRow(y);

            // Create all tiles to the right of the middle column.
            for (int tileCount = 0; tileCount < tilesOnEachSide; tileCount++)
            {

                // Increase y by the tile size.
                y += Constants.TILE_SIZE;

                // Create the row.
                CreateRow(y);

            }

            // Reset y to the middle column.
            y = 0.0f;

            // Create all tiles to the left of the middle column.
            for (int tileCount = 0; tileCount < tilesOnEachSide; tileCount++)
            {

                // Decrease y by the tile size.
                y -= Constants.TILE_SIZE;

                // Create the row.
                CreateRow(y);

            }

        }

        /// <summary>
        /// Create an entire row of tiles at a specified y position.
        /// </summary>
        /// <param name="y">Y position (column)</param>
        public void CreateRow(float y)
        {

            // Start at the middle of the row.
            float x = 0.0f;

            // Calculate tiles on each side of the center.
            int tilesOnEachSide = (NUM_TILES_WIDTH / 2) - 1 + NUM_BORDER_TILES;

            // Create the middle tile of the row.
            CreateTile(x, y);

            // Create all tiles to the right of the center.
            for (int tileCount = 0; tileCount < tilesOnEachSide; tileCount++)
            {

                // Increment x by the tile size.
                x += Constants.TILE_SIZE;

                // Create the tile.
                CreateTile(x, y);

            }

            // Reset x to the center.
            x = 0.0f;

            // Create all tiles to the left of the center.
            for (int tileCount = 0; tileCount < tilesOnEachSide; tileCount++)
            {

                // Decrement x by the tile size.
                x -= Constants.TILE_SIZE;

                // Create the tile.
                CreateTile(x, y);

            }
        }

        /// <summary>
        /// Create a tile at the specified x and y position.
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="z">Z position (y in 2D) </param>
        public void CreateTile(float x, float z)
        {

            //Create a new tile.
            MapTile newTile = new MapTile();

            // Set the position of the tile
            newTile.Transform.Position = new Vector3(x, Constants.FLOOR_HEIGHT - 1.0f, z);

            // Scale the tile to the correct size.
            newTile.Transform.Scale = new Vector3(Constants.TILE_SIZE, 1.0f, Constants.TILE_SIZE);

            // Get a random offset.
            float random = (float)rnd.NextDouble();

            // Apply the random offset to mitigate z fighting
            float yOffset = (random * (0.1f - (-0.1f))) + (-0.1f);

            // Add the new tile to the tile list
            AllMapTiles.Add(newTile);
        }

        /// <summary>
        /// Assign specific tiles to sections of the map.
        /// </summary>
        /// <param name="currentMatch">The current active match.</param>
        public void CreateDistinctTeamSections(Match currentMatch)
        {

            TeamTiles = new List<List<MapTile>>();

            // Iterate through all the map tiles.
            for (int i = 0; i < AllMapTiles.Count; i++)
            {

                // Iterate through all the team sections.
                for (int j = 0; j < currentMatch.teamSections.Count; j++)
                {

                    // Create a new list for this team.
                    List<MapTile> ThisTeamTiles = new List<MapTile>();

                    // If the current tile is in the bounds of the section.
                    if (currentMatch.teamSections[j].IsInBounds(AllMapTiles[i].Transform.Position))
                    {

                        // Tint the tile.
                        AllMapTiles[i].CurrentTint = currentMatch.teamSections[j].sectionColor;

                        // Add the tile to the tiles for this team.
                        ThisTeamTiles.Add(AllMapTiles[i]);

                    }

                    // Add the tiles for this team to the list of team tiles.
                    TeamTiles.Add(ThisTeamTiles);

                }
                
                // Check if this tile is in no man's land.
                if (currentMatch.NoMansLand.IsInBounds(AllMapTiles[i].Transform.Position))
                {

                    // Tint the tile.
                    AllMapTiles[i].CurrentTint = currentMatch.NoMansLand.sectionColor;

                }

            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            foreach (MapTile obj in AllMapTiles)
            {
                obj.Update(deltaTime);
            }
        }

        public override void Draw()
        {
            base.Draw();
            foreach (MapTile obj in AllMapTiles)
            {
                obj.Draw();
            }
        }
    }
}
