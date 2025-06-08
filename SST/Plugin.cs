using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.CustomItems.API;
using Exiled.CustomItems.API.Features;
using SST.Utilities;

namespace SST
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "SST";
        public override string Author => "Swishhyy";
        public override Version Version => new(0, 0, 0);// Gets the plugin version
        public static Plugin Instance { get; private set; } // Singleton instance for global access
        
        private EventHandler eventHandler;

        public bool IsUpdateInstalled => PluginUpdater.UpdateInstalled;

        public override void OnEnabled()
        {
            Instance = this;
            
            // Check for updates
            if (Config.AutoUpdateEnabled.ToLower() == "true")
            {
                Log.Info($"Checking for updates...");
                _ = Task.Run(PluginUpdater.RunAsync);
            }
            
            // Initialize and register event handlers
            eventHandler = new EventHandler();
            eventHandler.RegisterEvents();
            
            base.OnEnabled();
            Log.Info($"{Name} has been enabled!");
        }

        public override void OnDisabled()
        {
            // Unregister event handlers
            eventHandler?.UnregisterEvents();
            eventHandler = null;
            
            Instance = null;
            base.OnDisabled();
            Log.Info($"{Name} has been disabled!");
        }
    }
}
