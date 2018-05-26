﻿using System;
using System.Collections.Generic;
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
        public const float PLAYER_HEALTH = 100.0f;
        public const float PLAYER_MASS = 0.1f;
        public const float PLAYER_RADIUS = 3.0f;
        public const float PLAYER_SPEED = 25.0f;

        public Team Team { get; set; }
        public bool Dead { get; set; }
        public ToolType ToolEquipped { get; set; }

        // If the user is using the primary function of their tool or secondary
        public ToolMode ActiveToolMode { get; set; }

        public Vector3 moveRequest;

        public PlayerServer(Team team) : base(ObjectType.PLAYER, PLAYER_HEALTH, PLAYER_MASS, PLAYER_RADIUS, 0.0f, true)
        {
            Team = team;
            ToolEquipped = ToolType.BLOWER;
        }

        /// <summary>
        /// Updates the object, runs every frame.
        /// </summary>
        /// <param name="deltaTime">Time since lsat frame, in seconds.</param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            Vector3 newPlayerPos = Transform.Position + moveRequest * PLAYER_SPEED * deltaTime;
            newPlayerPos.Y = Constants.FLOOR_HEIGHT;


            TryMoveObject(newPlayerPos);

            //  Console.WriteLine("Tool equipped is " + ToolEquipped.ToString() + " and mode is " + ActiveToolMode.ToString());

        }

        /// <summary>
        /// Affects any object's within range of the player's tool.
        /// </summary>
        /// <param name="allObjects">A list of all objects in the game.</param>
        public void AffectObjectsInToolRange(List<GameObjectServer> allObjects)
        {

            // Itereate through all objects.
            for (int j = 0; j < allObjects.Count; j++)
            {

                //Get the current object.
                GameObjectServer gameObject = allObjects[j];

                // Check if it's within tool range, and that it's not the current player.
                if (gameObject != this && gameObject.IsInPlayerToolRange(this))
                {
                    // Hit the object.
                    gameObject.HitByTool(Transform.Position, ToolEquipped, ActiveToolMode);

                }
            }
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
        }

        /// <summary>
        /// Function that determines what happens when the player is hit by another player's tool.
        /// </summary>
        /// <param name="playerPosition">Position of the other player.</param>
        /// <param name="toolType">Type of tool hit by.</param>
        /// <param name="toolMode">Tool mode hit by.</param>
        public override void HitByTool(Vector3 playerPosition, ToolType toolType, ToolMode toolMode)
        {
            base.HitByTool(playerPosition, toolType, toolMode);
            // TODO
        }
    }
}
