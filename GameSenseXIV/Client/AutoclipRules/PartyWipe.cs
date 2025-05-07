using GameSenseXIV.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.Rules
{
    internal class PartyWipe : IAutoClipRule
    {
        public string RuleKey => "wipe";
        public string Label => "Party wipe";
        public string Description => "Triggers when the party wipes.";
        public bool Enabled
        {
            get { return Plugin.Configuration.ClipWipes; }
            set { Plugin.Configuration.ClipWipes = value; }
        }

        private Plugin Plugin { get; set; }

        public PartyWipe(Plugin plugin)
        {
            this.Plugin = plugin;
        }

        public void Dispose() { }

        public void SubscribeToEvents()
        {
            Plugin.DutyState.DutyWiped += OnDutyWipe;
        }

        public void UnsubscribeFromEvents()
        {
            Plugin.DutyState.DutyWiped -= OnDutyWipe;
        }

        private void OnDutyWipe(object? sender, ushort e)
        {
            Plugin.GSClient.Autoclip(this);
        }
    }
}
