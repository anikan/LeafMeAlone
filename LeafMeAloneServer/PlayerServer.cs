﻿using System;
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
        public const float PLAYER_HEALTH = 10.0f;
        public const float PLAYER_MASS = 0.1f;
        public const float PLAYER_RADIUS = 3.0f;
        public const float PLAYER_SPEED = 25.0f;

        public Team Team { get; set; }
        public bool Dead { get; set; }
        public ToolType ToolEquipped { get; set; }

        // If the user is using the primary function of their tool or secondary
        public ToolMode ActiveToolMode { get; set; }

        public Vector3 moveRequest;

        private Stopwatch deathClock = new Stopwatch();

        public PlayerServer(Team team) : base(ObjectType.PLAYER, PLAYER_HEALTH, PLAYER_MASS, PLAYER_RADIUS, 0.0f, true)
        {
            Team = team;
            ToolEquipped = ToolType.BLOWER;
            Burnable = true;
            JumpToRandomSpawn();

        }

        private void JumpToRandomSpawn()
        {
            Transform.Position = Team.GetNextSpawnPoint();
            foreach (ColliderObject obj in GameServer.instance.gameObjectDict.Values)
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
            Vector3 newPlayerPos = Transform.Position + moveRequest * PLAYER_SPEED * deltaTime;
            newPlayerPos.Y = Constants.FLOOR_HEIGHT;
            TryMoveObject(newPlayerPos);
            DoDeathLogic();
        }

        /// <summary>
        /// Handles player death
        /// </summary>
        private void DoDeathLogic()
        {
            // if health is down, start the players death clock
            if (Health < 0 && !Dead)
            {
                Dead = true;
                Burning = false;
                Burnable = false;
                Health = PLAYER_HEALTH;
                deathClock.Start();
                Collidable = false;
            // Once health is up, reset te death clock and player position
            } else if (Dead && deathClock.Elapsed.Seconds > Constants.DEATH_TIME)
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
                }
            }
        }

        /// <summary>
        /// Gets the transform of the active tool.
        /// </summary>
        /// <returns>Transform of the tool.</returns>
        public Transform GetToolTransform()
        {

            // TODO: Make this the actual tool transform.
            // Currently just the player transform.
            return Transform;

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
        /// Removes the player from the team 
        /// </summary>
        public override void Destroy()
        {
            Team.numPlayers--;
            base.Destroy();
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
