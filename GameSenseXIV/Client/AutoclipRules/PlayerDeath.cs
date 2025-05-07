using GameSenseXIV.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.Rules
{
    internal class PlayerDeath : IAutoClipRule
    {
        public string RuleKey => "death";
        public string Label => "Player death";
        public string Description => "Triggers when the local player dies.";
        public bool Enabled
        {
            get { return Plugin.Configuration.ClipDeaths; }
            set { Plugin.Configuration.ClipDeaths = value; }
        }

        private Plugin Plugin { get; set; }

        public PlayerDeath(Plugin plugin)
        {
            this.Plugin = plugin;
        }

        public void Dispose() { }

        public void SubscribeToEvents()
        {
            Plugin.OnDeath += OnDeath;
        }

        public void UnsubscribeFromEvents()
        {
            Plugin.OnDeath -= OnDeath;
        }

        private void OnDeath(object? sender, EventArgs e)
        {
            Plugin.GSClient.Autoclip(this);
        }
    }
}
