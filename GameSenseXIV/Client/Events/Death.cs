using GameSenseXIV.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.Events
{
    internal class Death : IAutoClipEvent
    {
        public string Name => "DEATH";
        public string Label => "Player Death";
        public string Description => "Triggers when the local player dies.";
        public int MinValue => 0;
        public int MaxValue => 100;
        public int IconId => (int)Icons.EventIcon.Dead;
        public string TimelineIconId => "DEATH";
        public int Previewable => 1;
        public bool ValueOptional => true;
        public bool Enabled
        {
            get { return Plugin.Configuration.ClipDeaths; }
            set { Plugin.Configuration.ClipDeaths = value; }
        }

        private Plugin Plugin { get; set; }

        public Death(Plugin plugin)
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
            Plugin.GSClient.SendGameEvent(this);

            if (Enabled)
            {
                Plugin.GSClient.Autoclip(this);
            }
        }
    }
}
