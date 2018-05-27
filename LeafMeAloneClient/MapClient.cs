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

        // Number of tiles beyond the treeline (so player can't see blue).
        private const int BORDER_TILES = 10;

        private const int NUM_TILES_WIDTH = (int)(Constants.MAP_WIDTH / Constants.TILE_SIZE) + BORDER_TILES;
        private const int NUM_TILES_HEIGHT = (int)(Constants.MAP_HEIGHT / Constants.TILE_SIZE) + BORDER_TILES;


        private List<MapTile> MapTiles;

        private Match activeMatch;

        /// <summary>
        /// Creates a new map with a map model.
        /// </summary>
        /// <param name="modelPath"></param>
        public MapClient() : base()
        {

            // Random number generator, to be used for y offsets. 
            rnd = new Random();
            activeMatch = Match.DefaultMatch;

            MapTiles = new List<MapTile>();


            // Iterate through the height of tiles.
            for (float y = -(NUM_TILES_HEIGHT * Constants.TILE_SIZE) / 2.0f; y < (NUM_TILES_HEIGHT * Constants.TILE_SIZE) / 2.0f; y += Constants.TILE_SIZE - 0.01f)
            {

                // Iterate through the width of tiles.
                for (float x = -(NUM_TILES_WIDTH * Constants.TILE_SIZE) / 2.0f; x < (NUM_TILES_WIDTH * Constants.TILE_SIZE) / 2.0f; x += Constants.TILE_SIZE - 0.01f)
                {

                    CreateTile(x, y);

                }
            }

            // Assign tiles to sections of the map
            CreateDistinctTeamSections(activeMatch);

        }

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
            MapTiles.Add(newTile);
        }

        /// <summary>
        /// Assign specific tiles to sections of the map.
        /// </summary>
        /// <param name="currentMatch">The current active match.</param>
        public void CreateDistinctTeamSections(Match currentMatch)
        {

            // Iterate through all the map tiles.
            for (int i = 0; i < MapTiles.Count; i++)
            {

                // Iterate through all the team sections.
                for (int j = 0; j < currentMatch.teamSections.Count; j++)
                {

                    // If the current tile is in the bounds of the section.
                    if (currentMatch.teamSections[j].IsInBounds(MapTiles[i].Transform.Position))
                    {

                        // Tint the tile.
                        MapTiles[i].CurrentTint = currentMatch.teamSections[j].sectionColor;

                    }
                }
                
                // Check if this tile is in no man's land.
                if (currentMatch.NoMansLand.IsInBounds(MapTiles[i].Transform.Position))
                {

                    // Tint the tile.
                    MapTiles[i].CurrentTint = currentMatch.NoMansLand.sectionColor;

                }

            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            foreach (MapTile obj in MapTiles)
            {
                obj.Update(deltaTime);
            }
        }

        public override void Draw()
        {
            base.Draw();
            foreach (MapTile obj in MapTiles)
            {
                obj.Draw();
            }
        }
    }
}
