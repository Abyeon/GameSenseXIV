using GameSenseXIV.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.Events
{
    internal class Clear : IAutoClipEvent
    {
        public string Name => "CLEAR";
        public string Label => "Duty Complete";
        public string Description => "Triggers when you complete a duty.";
        public int MinValue => 0;
        public int MaxValue => 100;
        public int IconId => (int)Icons.EventIcon.Kills;
        public string TimelineIconId => "WIN";
        public int Previewable => 1;
        public bool ValueOptional => true;
        public bool Enabled
        {
            get { return Plugin.Configuration.ClipClears; }
            set { Plugin.Configuration.ClipClears = value; }
        }

        private Plugin Plugin { get; set; }

        public Clear(Plugin plugin)
        {
            this.Plugin = plugin;
        }

        public void Dispose() { }

        public void SubscribeToEvents()
        {
            Plugin.DutyState.DutyCompleted += DutyCompleted;
        }

        public void UnsubscribeFromEvents()
        {
            Plugin.DutyState.DutyCompleted -= DutyCompleted;
        }

        private void DutyCompleted(object? sender, ushort e)
        {
            Plugin.GSClient.SendGameEvent(this);

            if (Enabled)
            {
                Plugin.GSClient.Autoclip(this);
            }
        }
    }
}
