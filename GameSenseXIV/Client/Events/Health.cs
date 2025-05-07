using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.Events
{
    internal class Health : IGameEvent
    {
        public string Name => "HEALTH";
        public string Label => "Health";
        public string Description => "Triggers whenever the player's health changes.";
        public int MinValue => 0;
        public int MaxValue => 100;
        public int IconId => (int)Icons.EventIcon.Health;
        public bool ValueOptional => false;

        private Plugin Plugin { get; set; }

        public Health(Plugin plugin)
        {
            this.Plugin = plugin;
        }

        public void Dispose() { }

        public void SubscribeToEvents()
        {
            Plugin.OnHealthChanged += OnHealthChanged;
        }

        public void UnsubscribeFromEvents()
        {
            Plugin.OnHealthChanged -= OnHealthChanged;
        }

        private int lastHPChange;

        private void OnHealthChanged(object? sender, uint currentHP)
        {
            if (sender == null) return;

            IPlayerCharacter character = (IPlayerCharacter)sender;
            double unFloored = (currentHP / character.MaxHp) * 100;

            // Convert to 0-100 range
            int converted = (int)Math.Floor(unFloored);

            if (lastHPChange != converted)
            {
                lastHPChange = converted;

                // Send event
                Plugin.Log.Debug($"Current HP: {currentHP}, LastHP: {lastHPChange}, Converted: {converted}");
                Plugin.GSClient.SendGameEvent(this, converted);
            }
        }
    }
}
