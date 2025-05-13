using Dalamud.Game.ClientState.Objects.SubKinds;
using GameSenseXIV.Client.Handlers;
using static GameSenseXIV.Client.Wrappers;
using Newtonsoft.Json;
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
        public object[]? Handlers { get; set; }

        private Plugin Plugin { get; set; }

        public Health(Plugin plugin)
        {
            this.Plugin = plugin;

            var datas = new List<object>();
            var frameLines = new List<Line>();

            frameLines.Add(new Line(true, "headline", true, null));
            frameLines.Add(new Line(true, "subline", null, null, "HP: "));
            frameLines.Add(new Line(false, "progress", null, true));

            datas.Add(new
            {
                lines = frameLines.ToArray()
            });

            Screened screened = new Screened();
            screened.Datas = datas.ToArray();

            this.Handlers = new object[] { screened };
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
            float maxHp = (float)character.MaxHp;

            // Convert to 0-100 range
            float unFloored = (float)currentHP / maxHp * 100f;
            int converted = (int)unFloored;

            if (lastHPChange != converted)
            {
                lastHPChange = converted;

                var data = new
                {
                    frame = new
                    {
                        headline = "Health",
                        subline = currentHP,
                        progress = (int)converted
                    }
                };

                Plugin.GSClient.SendGameEvent(this, data);
            }
        }
    }
}
