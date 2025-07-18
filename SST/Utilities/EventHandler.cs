﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using SST.Utilities;

namespace SST
{
    public class EventHandler
    {
        private const string LogPrefix = "[Auto-Updater] "; // Prefix for all auto-updater log messages

        public void RegisterEvents()
        {
            // Register the RoundStarted event
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        }

        public void UnregisterEvents()
        {
            // Unregister the RoundStarted event
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        }

        private void OnRoundStarted()
        {
            // Access the update status directly
            if (PluginUpdater.UpdateInstalled)
            {
                Log.Info($"{LogPrefix}Newest Update installed. Please Restart the server to apply the update!");
            }
        }
    }
}
