using SlimDX;

namespace Shared
{
    /// <summary>
    /// Constants for the game.
    /// </summary>
    public static class Constants
    {
        public static Vector3 PlayerToToolOffset = new Vector3(1.8f, 3.85f, 3.0f);

        //arrow for following player.
        public const string Arrow = @"../../Textures/Arrow.png";
        public const string RedHealth = @"../../Textures/RedHealth.png";
        public const string BlueHealth = @"../../Textures/BlueHealth.png";

        /// <summary>
        /// Player animation paths
        /// </summary>
        public const string PlayerWalkBlowerAnim = @"../../Models/PlayerModel/Walk_Leafblower.dae";
        public const string PlayerWalkThrowerAnim = @"../../Models/PlayerModel/Walk_Heavy_Flamethrower.dae";
        public const string PlayerIdleAnim = @"../../Models/PlayerModel/Tail_Wag_Idle.dae";
        public const string PlayerVictoryAnim = @"../../Models/PlayerModel/Victory.dae";
        public const string PlayerDefeatAnim = @"../../Models/PlayerModel/Defeat.dae";
        public const string PlayerHurtAnim = @"../../Models/PlayerModel/Hurt.dae";


        /// <summary>
        /// Win/loss text.
        /// </summary>
        public const string WinText = "You Win!";
        public const string LoseText = "You Lose!";

        /// <summary>
        /// Shaders
        /// </summary>
        public const string DefaultShader = @"../../Shaders/defaultShader.fx";
        public const string ParticleShader = @"../../Shaders/particle.fx";

        /// <summary>
        /// Textures
        /// </summary>
        public const string FireTexture = @"../../Particles/fire_red.png";
        public const string WindTexture = @"../../Particles/Wind_Transparent2.png";


        /// <summary>
        /// Models
        /// </summary>
        public const string LeafModel = @"../../Models/LeafModel/05.13.18_Leaf.fbx";
        public const string PlayerModel = @"../../Models/PlayerModel/05.03.18_Version2.fbx";
        public const string DefaultMapModel = @"../../Models/Terrain/Terrain.fbx";
        public const string TreeModel = @"../../Models/Tree/TreeAttempt.fbx";

        /// <summary>
        /// Sounds/Audio files
        /// </summary>
        public const string Bgm = @"../../Sound/OptionalForestAmbient.wav";
        public const string FlameThrowerStart = @"../../Sound/FlameThrower_Start.wav";
        public const string FlameThrowerLoop = @"../../Sound/FlameThrower_Loop.wav";
        public const string FlameThrowerEnd = @"../../Sound/FlameThrower_End.wav";

        public const string LeafBlowerStart = @"../../Sound/LeafBlower_Start.wav";
        public const string LeafBlowerLoop = @"../../Sound/LeafBlower_Loop.wav";
        public const string LeafBlowerEnd = @"../../Sound/LeafBlower_End.wav";

        public const string SuctionStart = @"../../Sound/Suction_Start.wav";
        public const string SuctionLoop = @"../../Sound/Suction_Loop.wav";
        public const string SuctionEnd = @"../../Sound/Suction_End.wav";

        public const string LeafIgniting = @"../../Sound/Leaf_Igniting.wav";
        public const string LeafBurning = @"../../Sound/Leaf_Burning.wav";
        public const string LeafBurnup = @"../../Sound/Leaf_Burnup.wav";
        public const string LeafPutoff = @"../../Sound/Leaf_Putoff.wav";

        public const string PlayerFootstep = @"../../Sound/Footsteps.wav";

        /// <summary>
        /// Map Constants
        /// </summary>
        
        // Height of the world floor.
        public const float FLOOR_HEIGHT = -10.0f;

        // Width/height of the map.
        public const float MAP_WIDTH = 150.0f;
        public const float MAP_HEIGHT = 80.0f;

        // Map tile size information.
        public const float TILE_SIZE = 8.0f;

        // Percent of the map that's no man's land. Will be rounded to tile size.
        public const float NO_MANS_LAND_PERCENT = 0.25f;

        // Size of the map outside the treeline (to prevent players from seeing skybox).
        public const float OUTER_BORDER_SIZE = 40.0f;

        // Margin around the map for leaf spawning (within treeline). Leaves won't spawn in these margins.
        public const float BORDER_MARGIN = 5.0f;

        // Total number of leaves in the game.
        public const int NUM_LEAVES = 300;
        // Number of players in a regular match 
        public const int NUM_PLAYERS = 4;
        // Number of leaves to win a reg match
        public const int WIN_LEAF_NUM = 200;
        // Reg match time
        public const int MATCH_INIT_TIME = 3;
        public const int MATCH_TIME = 300 + MATCH_INIT_TIME;
        public const int MATCH_RESET_TIME = 10;

        public const float PUSH_FACTOR = 3.0f;
        public const float PLAYER_HEALTH = 10.0f;

        public const float BLOWER_DISTANCE_SCALER = 2.0f;

        public const float TREE_FREQUENCY = 0.01f;

        public const bool PIVOT_DEBUG = false;

        public const int DEATH_TIME = 3;

        // Constants for leafs. 
        public const float LEAF_HEALTH = 10.0f;
        public const float LEAF_MASS = 0.1f;
        public const float LEAF_RADIUS = 0.0f;
        public const float LEAF_BOUNCIENESS = 0.8f;

        public const float LEAF_ROTATE_SPEED = 0.2f;
    }
}
