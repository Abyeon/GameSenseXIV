using GameSenseXIV.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.Events
{
    internal class Wipe : IAutoClipEvent
    {
        public string Name => "WIPE";
        public string Label => "Party Wipe";
        public string Description => "Triggers when the party wipes.";
        public int MinValue => 0;
        public int MaxValue => 100;
        public int IconId => (int)Icons.EventIcon.Flash;
        public string TimelineIconId => "HEADSHOT";
        public int Previewable => 1;
        public bool ValueOptional => true;
        public bool Enabled
        {
            get { return Plugin.Configuration.ClipWipes; }
            set { Plugin.Configuration.ClipWipes = value; }
        }

        private Plugin Plugin { get; set; }

        public Wipe(Plugin plugin)
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
            Plugin.GSClient.SendGameEvent(this);

            if (Enabled)
            {
                Plugin.GSClient.Autoclip(this);
            }
        }
    }
}
