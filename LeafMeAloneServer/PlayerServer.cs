using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Packet;
using SlimDX;

namespace Server
{
    public class PlayerServer : PhysicsObject, IPlayer
    {

        // Constant values of the player.
        public const float PLAYER_MASS = 0.1f;
        public const float PLAYER_RADIUS = 3.0f;
        public const float PLAYER_SPEED = 25.0f;
        public const float THROWER_SPEED = 10.0f;

        private float currentSpeed = PLAYER_SPEED;

        public Team Team { get; set; }
        public bool Dead { get; set; }
        public ToolType ToolEquipped { get; set; }

        // If the user is using the primary function of their tool or secondary
        public ToolMode ActiveToolMode { get; set; }

        public Vector3 moveRequest;

        private Stopwatch deathClock = new Stopwatch();

        public PlayerServer(Team team) : base(ObjectType.PLAYER, Constants.PLAYER_HEALTH, PLAYER_MASS, 0.0f, true)
        {
            Team = team;
            ToolEquipped = ToolType.BLOWER;
            Burnable = true;
            Radius = PLAYER_RADIUS;
            colliderType = ColliderType.CIRCLE;
            JumpToRandomSpawn();

        }

        private void JumpToRandomSpawn()
        {
            Transform.Position = Team.GetNextSpawnPoint();
            foreach (ColliderObject obj in GameServer.instance.GetGameObjectList())
            {
                if (obj is PlayerServer || obj is TreeServer)
                {
                    if (obj != this && IsColliding(obj))
                    {
                        Transform.Position = Team.GetNextSpawnPoint();
                    }

                }
            }
        }

        /// <summary>
        /// Updates the object, runs every frame.
        /// </summary>
        /// <param name="deltaTime">Time since lsat frame, in seconds.</param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            // Move the player in accordance with requests
            Vector3 newPlayerPos = Transform.Position + moveRequest * currentSpeed * deltaTime;
            newPlayerPos.Y = Constants.FLOOR_HEIGHT;
            TryMoveObject(newPlayerPos);
            TryRespawn();

            // If player isn't burning and is low on health.
            if (!Burning && Health < Constants.PLAYER_HEALTH)
            {
                // Regen health.
                Health += Constants.HEALTH_REGEN_RATE * deltaTime;
            }

        }

        /// <summary>
        /// checks if the player is ready to respawn and does so.
        /// </summary>
        private void TryRespawn()
        {
            if (Dead && deathClock.Elapsed.Seconds > Constants.DEATH_TIME)
            {
                deathClock.Reset();
                Reset();
            }
        }

        /// <summary>
        /// Affects any object's within range of the player's tool.
        /// </summary>
        /// <param name="allObjects">A list of all objects in the game.</param>
        public void AffectObjectsInToolRange(List<GameObjectServer> allObjects)
        {

            // Iterate through all objects.
            for (int j = 0; j < allObjects.Count; j++)
            {

                //Get the current object.
                GameObjectServer gameObject = allObjects[j];

                if (ActiveToolMode == ToolMode.PRIMARY || ActiveToolMode == ToolMode.SECONDARY)
                {
                    // Check if it's within tool range, and that it's not the current player.
                    if (gameObject != this && gameObject.IsInPlayerToolRange(this))
                    {
                        // Hit the object.
                        gameObject.HitByTool(GetToolTransform(), ToolEquipped, ActiveToolMode);

                    }

                    if (ToolEquipped == ToolType.THROWER)
                    {

                        currentSpeed = THROWER_SPEED;

                    }
                    else
                    {
                        currentSpeed = PLAYER_SPEED;
                    }
                }
                else
                {
                    currentSpeed = PLAYER_SPEED;
                }
            }
        }

        /// <summary>
        /// Gets the transform of the active tool.
        /// </summary>
        /// <returns>Transform of the tool.</returns>
        public Transform GetToolTransform()
        {
            Matrix mat = Matrix.RotationX(Transform.Rotation.X) *
                       Matrix.RotationY(Transform.Rotation.Y) *
                       Matrix.RotationZ(Transform.Rotation.Z);

            Transform toolTransform = new Transform();
            toolTransform.Position = Transform.Position + Vector3.TransformCoordinate(Constants.PlayerToToolOffset, mat);
            toolTransform.Rotation = Transform.Rotation;

            // TODO: Make this the actual tool transform.
            // Currently just the player transform.

            return toolTransform;

        }


        /// <summary>
        /// Update the player based on a packet sent from the client.
        /// </summary>
        /// <param name="packet">Packet from client.</param>
        public void UpdateFromPacket(RequestPacket packet)
        {
            //Save movement request and normalize it so that we only move once per tick.
            moveRequest = new Vector3(packet.DeltaX, 0.0f, packet.DeltaZ);
            moveRequest.Normalize();

            Transform.Rotation.Y = packet.DeltaRot;

            if (packet.ToolRequest != ToolType.SAME)
            {
                ToolEquipped = packet.ToolRequest;
            }

            ActiveToolMode = packet.ToolMode;

            if (Dead)
            {
                ActiveToolMode = ToolMode.NONE;
            }
        }

        /// <summary>
        /// Function that determines what happens when the player is hit by another player's tool.
        /// </summary>
        /// <param name="toolTransform">Position of the other player.</param>
        /// <param name="toolType">Type of tool hit by.</param>
        /// <param name="toolMode">Tool mode hit by.</param>
        public override void HitByTool(Transform toolTransform, ToolType toolType, ToolMode toolMode)
        {

            if (!Dead)
            {
                base.HitByTool(toolTransform, toolType, toolMode);
            }
        }

        /// <summary>
        /// Kill the player
        /// </summary>
        public override void Die()
        {
            Dead = true;
            Burning = false;
            Burnable = false;
            Health = Constants.PLAYER_HEALTH;
            deathClock.Start();
            Collidable = false;
        }

        /// <summary>
        /// Resets the player to a specified position
        /// </summary>
        /// <param name="pos">The position to set the player to</param>
        internal void Reset()
        {
            Velocity = new Vector3();
            moveRequest = new Vector3();
            JumpToRandomSpawn();
            Health = Constants.PLAYER_HEALTH;
            Burning = false;
            Dead = false;
            Burnable = true;
            Collidable = true;
            ActiveToolMode = ToolMode.NONE;
        }
    }
}
