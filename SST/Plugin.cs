using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.CustomItems.API;
using Exiled.CustomItems.API.Features;

namespace SST
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "SST";
        public override string Author => "Swishhyy";
        public override Version Version => new(1, 0, 0);// Gets the plugin version
        public static Plugin Instance { get; private set; } // Singleton instance for global access

    }
}
