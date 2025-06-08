// -----------------------------------------------------------------------
// <copyright file="AutoUpdater.cs" company="SST">
// Copyright (c) Swishhyy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json.Linq;

namespace SST.Utilities
{
    public static class AutoUpdater
    {
        private const string RepoOwner = "Swishhyy";
        private const string RepoName = "SST";
        private const string PluginFileName = "SST.dll";
        private const string LogPrefix = "[Auto-Updater] ";

        private static readonly string GithubApiLatestRelease = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
        private static readonly string PluginPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PluginFileName);
        
        public static bool UpdateInstalled { get; private set; } = false;

        public static async Task RunAsync()
        {
            try
            {
                Log.Info($"{LogPrefix}Checking GitHub for plugin updates...");

                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "SST-AutoUpdater");

                var response = await client.GetAsync(GithubApiLatestRelease);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var releaseInfo = JObject.Parse(json);

                string latestVersion = releaseInfo["tag_name"]?.ToString().TrimStart('v');
                string currentVersion = Plugin.Instance.Version.ToString();

                if (string.IsNullOrWhiteSpace(latestVersion))
                {
                    Log.Warning($"{LogPrefix}Could not determine latest version tag.");
                    return;
                }

                Log.Info($"{LogPrefix}Current version: {currentVersion}, Latest version: {latestVersion}");

                if (!IsUpdateAvailable(currentVersion, latestVersion))
                {
                    Log.Info($"{LogPrefix}Plugin is up to date.");
                    return;
                }

                Log.Warning($"{LogPrefix}Update available: {latestVersion} (current: {currentVersion})");

                if (Plugin.Instance.Config.AutoUpdateEnabled.ToLower() != "true")
                {
                    Log.Info($"{LogPrefix}Auto-update is disabled. Please update manually.");
                    return;
                }

                // Get the download URL for the latest release asset
                string downloadUrl = null;
                JArray assets = (JArray)releaseInfo["assets"];
                
                foreach (JToken asset in assets)
                {
                    string assetName = asset["name"].ToString();
                    if (assetName.EndsWith(".dll") && assetName.Contains("SST"))
                    {
                        downloadUrl = asset["browser_download_url"].ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Log.Warning($"{LogPrefix}Plugin .dll not found in the release assets.");
                    return;
                }

                string tempPath = PluginPath + ".update";

                Log.Info($"{LogPrefix}Downloading updated plugin...");

                using (var downloadStream = await client.GetStreamAsync(downloadUrl))
                using (var fileStream = File.Create(tempPath))
                {
                    await downloadStream.CopyToAsync(fileStream);
                }

                // Set the update installed flag
                UpdateInstalled = true;
                Log.Info($"{LogPrefix}Update downloaded successfully! Restart the server to apply.");
            }
            catch (Exception ex)
            {
                Log.Error($"{LogPrefix}Update failed: {ex.Message}");
            }
        }

        private static bool IsUpdateAvailable(string currentVersion, string latestVersion)
        {
            try
            {
                Version current = new Version(currentVersion);
                Version latest = new Version(latestVersion);
                return latest > current;
            }
            catch (Exception ex)
            {
                Log.Error($"{LogPrefix}Error comparing versions: {ex.Message}");
                return false;
            }
        }
    }
}