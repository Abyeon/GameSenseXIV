using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.Events
{
    internal class Death : IGameEvent
    {
        public string Name => "DEATH";
        public int MinValue => 0;
        public int MaxValue => 100;
        public int IconId => (int)Icons.EventIcon.Dead;
        public bool ValueOptional => true;

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
        }
    }
}
