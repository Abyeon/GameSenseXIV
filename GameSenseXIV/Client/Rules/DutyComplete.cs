using GameSenseXIV.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.Rules
{
    internal class DutyComplete : IAutoClipRule
    {
        public string RuleKey => "duty_complete";
        public string Label => "Duty Completion";
        public string Description => "Triggers when you complete a duty.";
        public bool Enabled
        {
            get { return Plugin.Configuration.ClipClears; }
            set { Plugin.Configuration.ClipClears = value; }
        }

        private Plugin Plugin { get; set; }

        public DutyComplete(Plugin plugin)
        {
            this.Plugin = plugin;
        }

        public void Dispose() { }

        public void SubscribeToEvents()
        {
            Plugin.DutyState.DutyCompleted += OnDutyCompleted;
        }

        public void UnsubscribeFromEvents()
        {
            Plugin.DutyState.DutyCompleted -= OnDutyCompleted;
        }

        private void OnDutyCompleted(object? sender, ushort e)
        {
            Plugin.GSClient.Autoclip(this);
        }
    }
}
