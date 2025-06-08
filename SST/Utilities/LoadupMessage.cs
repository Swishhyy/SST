using System;
using Exiled.API.Features;

namespace SST.Utilities
{
    /// <summary>
    /// Provides the ASCII art logo for the plugin.
    /// </summary>
    public static class LoadupMessage
    {
        /// <summary>
        /// Gets the SST ASCII art logo.
        /// </summary>
        public static string Message => @"
 _____ _____ _____ 
/  ___/  ___|_   _|
\ `--.\ `--.  | |  
 `--. \`--. \ | |   by Swishhyy
/\__/ /\__/ / | |  
\____/\____/  \_/  
";

        /// <summary>
        /// Displays the startup message in the console.
        /// </summary>
        public static void DisplayStartupMessage()
        {
            // Get the plugin version to append
            string version = Plugin.Instance?.Version.ToString() ?? "Unknown";
            
            // Complete message with version
            string fullMessage = Message + "\nv" + version;
            
            // Log to console with color
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(fullMessage);
            Console.ResetColor();
            
            // Log to Exiled logs
            Log.Info(fullMessage);
        }
    }
}
