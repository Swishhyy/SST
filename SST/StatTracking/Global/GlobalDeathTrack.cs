using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using SST.Utilities;

// Checks individual player deaths & then global averages aswell
// then exports to an API via GlobalStatTracker

namespace SST.StatTracking.Global
{
    internal class GlobalDeathTrack
    {
        // Dictionary to track deaths per player: <player ID, death count>
        private Dictionary<string, int> playerDeaths = new Dictionary<string, int>();
        
        // Dictionary to track deaths by damage type: <damage type, death count>
        private Dictionary<string, int> deathsByDamageType = new Dictionary<string, int>();
        
        // Total deaths tracked across all players
        private int totalDeaths = 0;
        
        // Reference to global stat tracker for exporting data
        private GlobalStatTracker statTracker;

        public GlobalDeathTrack(GlobalStatTracker tracker)
        {
            statTracker = tracker;
            RegisterEvents();
        }

        public void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
        }

        public void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
        }

        private void OnPlayerDied(DiedEventArgs ev)
        {
            string playerId = ev.Player.UserId;
            string damageType = ev.DamageHandler.Type.ToString();
            
            // Increment death count for the player
            if (playerDeaths.ContainsKey(playerId))
                playerDeaths[playerId]++;
            else
                playerDeaths.Add(playerId, 1);
            
            // Increment death count for the damage type
            if (deathsByDamageType.ContainsKey(damageType))
                deathsByDamageType[damageType]++;
            else
                deathsByDamageType.Add(damageType, 1);
            
            // Increment total death count
            totalDeaths++;
            
            // Log for debugging
            Log.Debug($"Death recorded: {ev.Player.Nickname} died from {damageType}");
            
            // Update statistics in the global tracker
            UpdateStatistics();
        }

        /// <summary>
        /// Updates statistics in the global stat tracker
        /// </summary>
        private void UpdateStatistics()
        {
            // Example implementation - would need to be adapted based on GlobalStatTracker's actual API
            // statTracker.UpdateDeathStatistics(playerDeaths, deathsByDamageType, totalDeaths, GetAverageDeathsPerPlayer());
        }

        /// <summary>
        /// Gets the players with the most deaths sorted by death count
        /// </summary>
        /// <param name="count">Number of players to return</param>
        public Dictionary<string, int> GetMostDeathsPlayers(int count = 10)
        {
            return playerDeaths
                .OrderByDescending(pair => pair.Value)
                .Take(count)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// Gets the most common causes of death sorted by death count
        /// </summary>
        /// <param name="count">Number of causes to return</param>
        public Dictionary<string, int> GetMostCommonDeathCauses(int count = 10)
        {
            return deathsByDamageType
                .OrderByDescending(pair => pair.Value)
                .Take(count)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// Gets the death count for a specific player
        /// </summary>
        /// <param name="playerId">Player's unique ID</param>
        public int GetPlayerDeathCount(string playerId)
        {
            return playerDeaths.ContainsKey(playerId) ? playerDeaths[playerId] : 0;
        }

        /// <summary>
        /// Gets the global average of deaths per player
        /// </summary>
        public float GetAverageDeathsPerPlayer()
        {
            if (playerDeaths.Count == 0)
                return 0;
                
            return (float)totalDeaths / playerDeaths.Count;
        }

        /// <summary>
        /// Gets the total number of deaths recorded
        /// </summary>
        public int GetTotalDeaths()
        {
            return totalDeaths;
        }

        /// <summary>
        /// Resets all death statistics (typically done at the end of a round or game)
        /// </summary>
        public void ResetStatistics()
        {
            playerDeaths.Clear();
            deathsByDamageType.Clear();
            totalDeaths = 0;
            
            Log.Debug("Death statistics have been reset");
        }
    }
}
