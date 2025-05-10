using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client
{
    internal class Wrappers
    {
        public class AutoclipRule
        {
            [JsonProperty("rule_key")]
            public string Key;

            [JsonProperty("label")]
            public string Label;

            [JsonProperty("enabled")]
            public bool Enabled;

            public AutoclipRule(string key, string label, bool enabled)
            {
                Key = key;
                Label = label;
                Enabled = enabled;
            }
        }

        public class GameMetadata
        {
            [JsonProperty("game")]
            public string Game;

            [JsonProperty("game_display_name")]
            public string DisplayName;

            [JsonProperty("developer")]
            public string Developer;

            public GameMetadata(string game, string displayName, string developer)
            {
                Game = game;
                DisplayName = displayName;
                Developer = developer;
            }
        }

        public class BoundGameEvent
        {
            [JsonProperty("game")]
            public string Game;

            [JsonProperty("event")]
            public string EventName;

            [JsonProperty("min_value")]
            public int MinValue;

            [JsonProperty("max_value")]
            public int MaxValue;

            [JsonProperty("icon_id")]
            public int IconId;

            [JsonProperty("value_optional")]
            public bool ValueOptional;

            [JsonProperty("handers")]
            public object[] Handlers;

            public BoundGameEvent(string game, string eventName, int minValue, int maxValue, int iconId, bool valueOptional, object[]? handlers)
            {
                this.Game = game;
                this.EventName = eventName;
                this.MinValue = minValue;
                this.MaxValue = maxValue;
                this.IconId = iconId;
                this.ValueOptional = valueOptional;
                this.Handlers = handlers;
            }
        }

        public class RegisteredGameEvent
        {
            [JsonProperty("game")]
            public string Game;

            [JsonProperty("event")]
            public string EventName;

            [JsonProperty("min_value")]
            public int MinValue;

            [JsonProperty("max_value")]
            public int MaxValue;

            [JsonProperty("icon_id")]
            public int IconId;

            [JsonProperty("value_optional")]
            public bool ValueOptional;

            [JsonProperty("handlers", NullValueHandling = NullValueHandling.Ignore)]
            public object[] Handlers;

            public RegisteredGameEvent(string game, string eventName, int minValue, int maxValue, int iconId, bool valueOptional, object[]? handlers)
            {
                this.Game = game;
                this.EventName = eventName;
                this.MinValue = minValue;
                this.MaxValue = maxValue;
                this.IconId = iconId;
                this.ValueOptional = valueOptional;
                this.Handlers = handlers;
            }
        }
    }
}
