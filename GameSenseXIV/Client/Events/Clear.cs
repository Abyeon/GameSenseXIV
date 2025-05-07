using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.Events
{
    internal class Clear : IGameEvent
    {
        public string Name => "CLEAR";
        public int MinValue => 0;
        public int MaxValue => 100;
        public int IconId => (int)Icons.EventIcon.Kills;
        public bool ValueOptional => true;

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
        }
    }
}
