using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using SST.Utilities;

// Checks individual player kills & then global averages aswell
// then exports to an API via GlobalStatTracker

namespace SST.StatTracking.Global
{
    internal class GlobalKillTrack
    {
        // Dictionary to track kills per player: <player ID, kill count>
        private Dictionary<string, int> playerKills = new Dictionary<string, int>();
        
        // Dictionary to track kills by weapon: <weapon name, kill count>
        private Dictionary<string, int> weaponKills = new Dictionary<string, int>();
        
        // Total kills tracked across all players
        private int totalKills = 0;
        
        // Reference to global stat tracker for exporting data
        private GlobalStatTracker statTracker;

        public GlobalKillTrack(GlobalStatTracker tracker)
        {
            statTracker = tracker;
            RegisterEvents();
        }

        public void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurting;
        }

        public void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;
        }

        private void OnPlayerDied(DiedEventArgs ev)
        {
            // The Died event doesn't directly tell us who the killer is
            // We'll just record the death here
            Log.Debug($"Player died: {ev.Player.Nickname}");
        }

        private void OnPlayerHurting(HurtingEventArgs ev)
        {
            // Check if this damage would result in a kill (health after damage would be <= 0)
            if (ev.Player.Health - ev.Amount <= 0 && ev.Attacker != null && ev.Attacker != ev.Player)
            {
                string attackerId = ev.Attacker.UserId;
                string damageType = ev.DamageHandler.Type.ToString();
                
                // Increment kill count for the player
                if (playerKills.ContainsKey(attackerId))
                    playerKills[attackerId]++;
                else
                    playerKills.Add(attackerId, 1);
                
                // Increment kill count for the weapon
                if (weaponKills.ContainsKey(damageType))
                    weaponKills[damageType]++;
                else
                    weaponKills.Add(damageType, 1);
                
                // Increment total kill count
                totalKills++;
                
                Log.Debug($"Kill recorded: {ev.Attacker.Nickname} killed {ev.Player.Nickname} with {damageType}");
                
                // Update the global statistics tracker
                UpdateStatistics();
            }
        }

        /// <summary>
        /// Updates statistics in the global stat tracker
        /// </summary>
        private void UpdateStatistics()
        {
            // Example implementation - would need to be adapted based on GlobalStatTracker's actual API
            // statTracker.UpdateKillStatistics(playerKills, weaponKills, totalKills, GetAverageKillsPerPlayer());
        }

        public Dictionary<string, int> GetTopKillers(int count = 10)
        {
            return playerKills
                .OrderByDescending(pair => pair.Value)
                .Take(count)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public Dictionary<string, int> GetMostUsedWeapons(int count = 10)
        {
            return weaponKills
                .OrderByDescending(pair => pair.Value)
                .Take(count)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public int GetPlayerKillCount(string playerId)
        {
            return playerKills.ContainsKey(playerId) ? playerKills[playerId] : 0;
        }

        public float GetAverageKillsPerPlayer()
        {
            if (playerKills.Count == 0)
                return 0;
                
            return (float)totalKills / playerKills.Count;
        }

        public void ResetStatistics()
        {
            playerKills.Clear();
            weaponKills.Clear();
            totalKills = 0;
            Log.Debug("Kill statistics have been reset");
        }
    }
}
