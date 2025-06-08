using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.CustomItems.API;
using Exiled.CustomItems.API.Features;
using Newtonsoft.Json.Linq;
using SST.Utilities;

namespace SST
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "SST";
        public override string Author => "Swishhyy";
        
        // Use 0.0.0 to ensure we can detect the GitHub 1.0.0 as a newer version
        public override Version Version => new Version(1, 0, 0);
        
        public override Version RequiredExiledVersion => new(9, 6, 0);
        public static Plugin Instance { get; private set; }

        private EventHandler eventHandler;

        public bool IsUpdateInstalled => PluginUpdater.UpdateInstalled;

        public override void OnEnabled()
        {
            Instance = this;

            if (RequiredExiledVersion > Exiled.Loader.Loader.Version)
            {
                Log.Error($"{Exiled.Loader.Loader.Version} is installed. Please update Exiled API to {RequiredExiledVersion} to use this plugin.");
                Log.Error($"{Name} will be disabled to prevent potential issues you may face");
                return;
            }
            
            // Initialize and register event handlers
            eventHandler = new EventHandler();
            eventHandler.RegisterEvents();
            
            // Log plugin version for debugging
            Log.Info($"Plugin version from property: {Version}");
            Console.WriteLine($"[{Name}] Plugin version: {Version}");
            
            // Check for updates
            if (Config.AutoUpdateEnabled.ToLower() == "true")
            {
                Log.Info($"Checking for updates...");
                Console.WriteLine($"[{Name}] Starting update check process");
                
                // Run update check in a separate task, but with error handling
                Task.Run(async () => 
                {
                    try 
                    {
                        await PluginUpdater.RunAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{Name}] Update check task threw an exception: {ex.Message}");
                        Log.Error($"Update check task failed: {ex.Message}");
                    }
                });
            }
            else
            {
                Log.Info($"Auto-update is disabled in config.");
            }

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
