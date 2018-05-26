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

        // Tile width/height information.
        private const float TILE_WIDTH = 10.0f;
        private const float TILE_HEIGHT = 10.0f;

        // Number of tiles beyond the treeline (so player can't see blue).
        private const int BORDER_TILES = 10;

        private const int NUM_TILES_PER_SIDE = (int)(Constants.MAP_WIDTH / TILE_WIDTH) + BORDER_TILES;

        private List<MapTile> MapTiles;

        /// <summary>
        /// Creates a new map with a map model.
        /// </summary>
        /// <param name="modelPath"></param>
        public MapClient() : base()
        {

            Random rnd = new Random();

            MapTiles = new List<MapTile>();

            for (float y = -(NUM_TILES_PER_SIDE * TILE_HEIGHT) / 2.0f; y < (NUM_TILES_PER_SIDE * TILE_HEIGHT) / 2.0f; y += TILE_HEIGHT - 0.2f)
            {

                for (float x = -(NUM_TILES_PER_SIDE * TILE_WIDTH) / 2.0f; x < (NUM_TILES_PER_SIDE * TILE_WIDTH) / 2.0f; x +=TILE_WIDTH - 0.2f)
                {

                    MapTile newTile = new MapTile();

                    newTile.Transform.Position = new Vector3(x, Constants.FLOOR_HEIGHT - 1.0f, y);
                    newTile.Transform.Scale = new Vector3(TILE_WIDTH, 1.0f, TILE_HEIGHT);

                    float random = (float) rnd.NextDouble();

                    float yOffset = (random * (0.1f - (-0.1f))) + (-0.1f);


                    MapTiles.Add(newTile);

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
