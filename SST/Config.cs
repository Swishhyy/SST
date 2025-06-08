using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SST
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true; 
        public bool Debug { get; set; } = false;
        public string DiscordWebhookUrl { get; set; } = "";
        public string ServerGuildId { get; set; } = "";
        public string AutoUpdateEnabled { get; set; } = "true";
    }
}
