using Shared;
using Shared.Packet;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Server
{
    /// <summary>
    /// Handles the match logic for the server.
    /// </summary>
    internal class MatchHandler
    {
        private Stopwatch matchResetTimer; // the timer for match reset
        private Match match;
        private NetworkServer network;
        private GameServer game;
        private int playerCount = 0;


        /// <summary>
        /// Initializes the match handler.
        /// </summary>
        public MatchHandler(Match toHandle, NetworkServer networkHandler, GameServer game )
        {
            match = toHandle;
            network = networkHandler;
            this.game = game;
            matchResetTimer = new Stopwatch();
        }

        /// <summary>
        /// Starts the match and sends an update to all clients
        /// </summary>
        /// <param name="time">The time of match duration</param>
        public void StartMatch()
        {
            match.StartMatch(Constants.MATCH_TIME);
            network.SendAll(PacketUtil.Serialize(new MatchStartPacket(Constants.MATCH_TIME)));
        }

        /// <summary>
        /// Restarts the match by resetting the leaves and calling startmatch
        /// </summary>
        private void RestartMatch()
        {
            game.GetLeafListAsObjects().ForEach(l => l.Destroy());
            foreach (PlayerServer player in game.playerServerList)
            {
                player.Reset();
            }
            matchResetTimer.Reset();
            StartMatch();
        }

        /// <summary>
        /// Ends the match by sending out a match result packet and burning all the leaves, starting a reset timer
        /// </summary>
        /// <param name="winningTeam">The team that won the match</param>
        private void EndMatch(Team winningTeam)
        {
            BasePacket donePacket = new MatchResultPacket(winningTeam.name);
            network.SendAll(PacketUtil.Serialize(donePacket));
            game.GetLeafListAsObjects().ForEach(l => l.Burning = true);
            matchResetTimer.Start();
        }

        /// <summary>
        /// Returns whether the match is in the initialization stage or not
        /// </summary>
        /// <returns>Whether the match is initializing</returns>
        internal bool MatchInitializing()
        {
            return (match.Started() && match.GetTimeElapsed().Seconds < Constants.MATCH_INIT_TIME);
        }

        /// <summary>
        /// Checks the match status and responds accordingly.
        /// </summary>
        internal void DoMatchStatusUpdates()
        {
            // Check for match restart
            if (matchResetTimer.IsRunning && matchResetTimer.Elapsed.Seconds > Constants.MATCH_RESET_TIME)
            {
                matchResetTimer.Reset();
                RestartMatch();
            }

            // Check for match end
            match.CountObjectsOnSides(game.GetLeafListAsObjects());
            Team winningTeam = match.TryGameOver();
            if (winningTeam != null)
            {
                EndMatch(winningTeam);
            }
        }

        /// <summary>
        /// Gets the match object from the match handler
        /// </summary>
        /// <returns>The match object</returns>
        internal Match GetMatch()
        {
            return match;
        }

        internal PlayerServer AddPlayer()
        {
            PlayerServer newPlayer = new PlayerServer(match.teams[playerCount++ % match.teams.Count]);
            return newPlayer;
        }
    }
}