﻿using Shared;
using Shared.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.UI;

namespace Client
{
    /// <summary>
    /// Class which handles mapping the different packet types to client actions
    /// </summary>
    class ClientPacketHandler
    {
        private Dictionary<PacketType, Action<BasePacket>> packetHandlers;
        private GameClient client;

        /// <summary>
        /// Defines the different actions which the client will take upon recieving a packet, and defines 
        /// them as a dictionary that hashes by packet type
        /// </summary>
        /// <param name="client">The game client to fire callbacks for</param>
        public ClientPacketHandler(GameClient client)
        {
            this.client = client;

            // What to do during an update
            void UpdateObjectAction(ObjectPacket p)
            {
                NetworkedGameObjectClient packetObject = client.GetObjectFromPacket(p);
                packetObject.UpdateFromPacket(p);
            }

            // what to do during an update to the playerpacket 
            void UpdatePlayerAction(PlayerPacket p)
            {
                PlayerClient player = (PlayerClient)client.GetObjectFromPacket(p);
                if (client.GetActivePlayer() == player && p.Dead)
                {
                    client.DoPlayerDeath();
                }
                player.UpdateFromPacket(p);
            }

            // What to do during a destroy
            void DestroyAction(DestroyObjectPacket p)
            {
                NetworkedGameObjectClient packetObject = client.GetObjectFromPacket(p);
                packetObject.Die();
            }

            // What to do when creating an object
            GameObject CreateObjectAction(CreateObjectPacket p)
            {
                return client.CreateObjectFromPacket(p);
            }

            // What to do when creating a player
            void CreatePlayerAction(CreatePlayerPacket p)
            {
                PlayerClient player = (PlayerClient)CreateObjectAction(p.createPacket);
                player.PlayerTeam = p.team;
            }

            // What to do on game finish
            void GameResultAction(GameResultPacket p)
            {
                client.ResetGameTimer();
                if (client.GetPlayerTeam() == p.winningTeam)
                {
                    GlobalUIManager.GameWinLossState.SetState(UI.UIGameWLState.WinLoseState.Win);
                }
                else
                {
                    GlobalUIManager.GameWinLossState.SetState(UI.UIGameWLState.WinLoseState.Lose);
                }

                // Save the player stats to file when the game ends.
                GameClient.instance.SaveStats(GraphicsManager.ActivePlayer.stats);

                client.WinningTeam = p.winningTeam;
                client.PendingRematchState = true;
            }

            void GameStartAction(MatchStartPacket p)
            {
                client.StartMatchTimer(p.gameTime);
                GlobalUIManager.GameWinLossState.SetState(UI.UIGameWLState.WinLoseState.None);
                GlobalUIManager.GameWinLossState.SetStats(null);
                client.PendingRematchState = false;
                GlobalUIManager.Countdown.Start();
            }

            void SpectatorAction(SpectatorPacket p)
            {
                GraphicsManager.ActiveCamera.CameraPosition = new SlimDX.Vector3(0, 150, -1);
                GraphicsManager.ActiveCamera.CameraLookAt = new SlimDX.Vector3(0, 0, 0);
                client.CreateMap();
            }

            void StatReceiveAction(StatResultPacket p)
            {
                if (p.PlayerID == GraphicsManager.ActivePlayer.Id)
                    GraphicsManager.ActivePlayer.stats = p.stats;
            }

            packetHandlers = new Dictionary<PacketType, Action<BasePacket>>()
                {
                    {PacketType.CreatePlayerPacket, (p) => CreatePlayerAction((CreatePlayerPacket) p)},
                    {PacketType.CreateObjectPacket, (p) => CreateObjectAction((CreateObjectPacket) p) },
                    {PacketType.ObjectPacket, (p) => UpdateObjectAction((ObjectPacket) p) },
                    {PacketType.PlayerPacket, (p) => UpdatePlayerAction((PlayerPacket) p) },
                    {PacketType.DestroyObjectPacket, (p) => DestroyAction((DestroyObjectPacket) p)},
                    {PacketType.GameResultPacket, (p) => GameResultAction((GameResultPacket) p)},
                    {PacketType.MatchStartPacket, (p) => GameStartAction((MatchStartPacket)p)},
                    {PacketType.SpectatorPacket, (p) => SpectatorAction((SpectatorPacket)p)},
                    {PacketType.StatResultPacket, (p) => StatReceiveAction((StatResultPacket)p)},
                };
        }

        /// <summary>
        /// Fires a callback into the game client to handle a packet
        /// </summary>
        /// <param name="packet">The packet to handle</param>
        internal void HandlePacket(BasePacket packet)
        {
            packetHandlers.TryGetValue(packet.packetType, out Action<BasePacket> action);
            action.Invoke(packet);
        }
    }
}
