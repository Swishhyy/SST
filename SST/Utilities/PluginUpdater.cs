using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json.Linq;

namespace SST.Utilities
{
    public static class PluginUpdater
    {
        private const string RepoOwner = "Swishhyy";
        private const string RepoName = "SST";
        private const string PluginFileName = "SST.dll";
        public const string LogPrefix = "[Auto-Updater] ";

        // Use lazy initialization for the GitHub API URL to avoid static constructor exceptions
        private static readonly Lazy<string> LazyGithubApiLatestRelease = new Lazy<string>(() => $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
        private static string GithubApiLatestRelease => LazyGithubApiLatestRelease.Value;
        
        // Use Exiled's Paths.Plugins for a more reliable path
        private static string GetPluginPath() => Path.Combine(Paths.Plugins, PluginFileName);
        
        public static bool UpdateInstalled { get; private set; } = false;

        public static async Task RunAsync()
        {
            try
            {
                // Directly write to console to bypass any logging filters
                Console.WriteLine($"{LogPrefix} Starting update check...");
                Console.WriteLine($"{LogPrefix} API URL: {GithubApiLatestRelease}");
                
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "SST-AutoUpdater");
                
                // Direct log to match what's shown in your API-TEST logs
                Log.Info($"{LogPrefix}Checking for updates directly...");
                Console.WriteLine($"{LogPrefix} Sending request to GitHub API...");
                
                HttpResponseMessage response;
                try
                {
                    response = await client.GetAsync(GithubApiLatestRelease);
                    Console.WriteLine($"{LogPrefix} Received response: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{LogPrefix} HTTP request failed: {ex.Message}");
                    Log.Error($"{LogPrefix}GitHub API request failed: {ex.Message}");
                    return;
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"{LogPrefix} API request returned {response.StatusCode}");
                    Log.Error($"{LogPrefix}GitHub API request failed: {response.StatusCode}");
                    return;
                }
                
                string content;
                try
                {
                    content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"{LogPrefix} Received content length: {content.Length}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{LogPrefix} Failed to read response: {ex.Message}");
                    Log.Error($"{LogPrefix}Failed to read API response: {ex.Message}");
                    return;
                }
                
                JObject json;
                try
                {
                    json = JObject.Parse(content);
                    Console.WriteLine($"{LogPrefix} Successfully parsed JSON response");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{LogPrefix} Failed to parse JSON: {ex.Message}");
                    Log.Error($"{LogPrefix}Failed to parse JSON: {ex.Message}");
                    return;
                }
                
                string tagName = json["tag_name"]?.ToString();
                Console.WriteLine($"{LogPrefix} Found tag name: {tagName}");
                
                if (string.IsNullOrEmpty(tagName))
                {
                    Console.WriteLine($"{LogPrefix} Tag name is empty or null!");
                    Log.Error($"{LogPrefix}Couldn't find tag_name in GitHub response");
                    return;
                }
                
                Log.Info($"{LogPrefix}Latest release tag: {tagName}");
                
                // IMPORTANT FIX: Use Plugin.Instance.Version instead of Assembly version
                // This ensures we use the same version as what's defined in Plugin.cs
                string currentVersion = Plugin.Instance.Version.ToString();
                
                Console.WriteLine($"{LogPrefix} Current version (from Plugin): {currentVersion}, Latest version: {tagName}");
                Log.Info($"{LogPrefix}Current version: {currentVersion}, Latest version: {tagName}");
                
                // Compare the versions as Version objects to correctly handle version differences
                try 
                {
                    Version current = new Version(currentVersion);
                    Version latest = new Version(tagName);
                    
                    Console.WriteLine($"{LogPrefix} Comparing versions: {current} vs {latest}");
                    
                    if (latest <= current)
                    {
                        Console.WriteLine($"{LogPrefix} Plugin is up to date.");
                        Log.Info($"{LogPrefix}Plugin is up to date.");
                        return;
                    }
                    
                    Console.WriteLine($"{LogPrefix} Update available!");
                    Log.Warn($"{LogPrefix}Update available: {tagName} (current: {currentVersion})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{LogPrefix} Error comparing versions: {ex.Message}");
                    Log.Error($"{LogPrefix}Error comparing versions: {ex.Message}");
                    return;
                }
                
                // Find the asset to download
                JArray assets = json["assets"] as JArray;
                Console.WriteLine($"{LogPrefix} Found {assets?.Count ?? 0} assets");
                
                if (assets == null || assets.Count == 0)
                {
                    Console.WriteLine($"{LogPrefix} No assets found in release");
                    Log.Warn($"{LogPrefix}No assets found in release");
                    return;
                }
                
                string downloadUrl = null;
                string matchingAssetName = null;
                
                foreach (var asset in assets)
                {
                    string assetName = asset["name"]?.ToString();
                    Console.WriteLine($"{LogPrefix} Checking asset: {assetName}");
                    
                    if (assetName != null && assetName.Equals(PluginFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset["browser_download_url"]?.ToString();
                        matchingAssetName = assetName;
                        Console.WriteLine($"{LogPrefix} Found exact matching asset: {assetName}, URL: {downloadUrl}");
                        Log.Info($"{LogPrefix}Found asset: {assetName}, URL: {downloadUrl}");
                        break;
                    }
                    else if (assetName != null && assetName.EndsWith(".dll") && assetName.IndexOf("SST", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // Fallback to any DLL containing SST if exact match not found
                        downloadUrl = asset["browser_download_url"]?.ToString();
                        matchingAssetName = assetName;
                        Console.WriteLine($"{LogPrefix} Found similar asset: {assetName}, URL: {downloadUrl}");
                        Log.Info($"{LogPrefix}Found asset: {assetName}, URL: {downloadUrl}");
                    }
                }
                
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Console.WriteLine($"{LogPrefix} No matching .dll file found");
                    Log.Warn($"{LogPrefix}No matching .dll file found in release assets");
                    return;
                }
                
                // Get the plugin path using Exiled's Paths.Plugins
                string pluginPath = GetPluginPath();
                string tempPath = pluginPath + ".update";
                
                Console.WriteLine($"{LogPrefix} Plugin path: {pluginPath}");
                Console.WriteLine($"{LogPrefix} Temp path: {tempPath}");
                Log.Info($"{LogPrefix}Downloading to: {tempPath}");
                
                try
                {
                    // Download the file to a temporary location first
                    using (var downloadStream = await client.GetStreamAsync(downloadUrl))
                    using (var fileStream = File.Create(tempPath))
                    {
                        await downloadStream.CopyToAsync(fileStream);
                    }
                    
                    // Use the same approach as your friend's plugin - try to replace directly
                    try
                    {
                        Console.WriteLine($"{LogPrefix} Attempting direct file replacement...");
                        
                        // Delete the old file if it exists
                        if (File.Exists(pluginPath))
                        {
                            File.Delete(pluginPath);
                            Console.WriteLine($"{LogPrefix} Deleted old plugin file");
                        }
                        
                        // Move the update file to the final location
                        File.Move(tempPath, pluginPath);
                        Console.WriteLine($"{LogPrefix} Moved update file to final location");
                        
                        UpdateInstalled = true;
                        Console.WriteLine($"{LogPrefix} Update installed successfully!");
                        Log.Info($"{LogPrefix}Update installed successfully! The changes will take effect when the server restarts.");
                    }
                    catch (IOException ioEx)
                    {
                        // If direct replacement fails (due to file being locked), keep the .update file for Exiled to handle
                        Console.WriteLine($"{LogPrefix} Direct replacement failed (likely file locked): {ioEx.Message}");
                        Log.Warn($"{LogPrefix}Could not replace the plugin file directly. The update will be applied on server restart.");
                        
                        UpdateInstalled = true;
                        Console.WriteLine($"{LogPrefix} Update downloaded successfully! Will be applied on restart.");
                        Log.Info($"{LogPrefix}Update downloaded successfully! Restart the server to apply.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{LogPrefix} Failed to download update: {ex.Message}");
                    Log.Error($"{LogPrefix}Failed to download update: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{LogPrefix} Update check failed: {ex.Message}");
                Console.WriteLine($"{LogPrefix} Stack trace: {ex.StackTrace}");
                Log.Error($"{LogPrefix}Update check failed: {ex.Message}");
            }
            
            // Force log all information at the end to ensure it's displayed
            Console.WriteLine($"{LogPrefix} Update check completed. UpdateInstalled={UpdateInstalled}");
        }
    }
}
