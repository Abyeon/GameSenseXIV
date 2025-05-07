using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameSenseXIV.Client;
using GameSenseXIV.Client.Events;
using Newtonsoft.Json;

namespace GameSenseXIV.Services
{
    public class GameSense : IDisposable
    {
        string Game { get; init; }
        string GameDisplayName {  get; init; }
        string Developer { get; init; }
        uint HeartbeatDelay { get; init; }

        //internal List<IAutoClipEvent> AutoClipRules = new List<IAutoClipEvent>();
        internal List<IGameEvent> GameEvents = new List<IGameEvent>();
        //internal List<ITimelineEvent> TimelineEvents = new List<ITimelineEvent>();

        private Uri Address { get; init; }
        private HttpClient httpClient { get; set; }
        private Timer heartbeatTimer { get; set; }
        private DateTime lastClip {  get; set; }
        private Plugin Plugin { get; set; }

        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient?.Dispose();
            }

            heartbeatTimer.Dispose();

            //foreach (var rule in AutoClipRules)
            //{
            //    rule.UnsubscribeFromEvents();
            //    rule.Dispose();
            //}

            foreach (IGameEvent gameEvent in GameEvents)
            {
                gameEvent.UnsubscribeFromEvents();
                gameEvent.Dispose();
            }
        }

        public GameSense(Plugin plugin, string game, string gameDisplayName, string developer, uint heartbeatDelay = 14000)
        {
            this.Plugin = plugin;
            this.Game = game;
            this.GameDisplayName = gameDisplayName;
            this.Developer = developer;
            this.HeartbeatDelay = heartbeatDelay;
            this.lastClip = DateTime.MinValue;

            string filePath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SteelSeries", "SteelSeries Engine 3", "coreProps.json");
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // No idea if this works, just going to wait until the unlikely OSX Dalamud & SteelSeries user complains :D
                filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SteelSeries Engine 3", "coreProps.json");
            } else
            {
                throw new FileNotFoundException("Not running a compatible OS!");
            }

            // Get SteelSeries json
            using (StreamReader r  = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                CoreProps? props = JsonConvert.DeserializeObject<CoreProps>(json);

                if (props != null)
                {
                    Plugin.Log.Debug("Found SteelSeries address: " + props.address);
                    this.Address = new Uri($"http://{props.address}");
                    Plugin.Log.Debug(this.Address.ToString());

                    // Initiate the Http client
                    httpClient = new HttpClient()
                    {
                        BaseAddress = this.Address
                    };

                    // Register game
                    RegisterGame();

                    //// Add Autoclip rules
                    //AutoClipRules = new List<IAutoClipEvent>
                    //{
                    //    new PlayerDeath(this.Plugin),
                    //    new PartyWipe(this.Plugin),
                    //    new DutyComplete(this.Plugin)
                    //};

                    //// Add Game Events
                    //GameEvents = new List<IGameEvent>
                    //{
                    //    new Health(this.Plugin),
                    //    new Death(this.Plugin),
                    //    new Clear(this.Plugin)
                    //};

                    //// Add Timeline Events
                    //TimelineEvents = new List<ITimelineEvent>
                    //{
                    //    new TimelineDeath(),
                    //    new TimelineClear()
                    //};

                    // Register them
                    //RegisterAutoclips();
                    //RegisterGameEvents();
                    //RegisterTimelineEvents();

                    // Add Game Events
                    GameEvents = new List<IGameEvent>
                    {
                        new Clear(Plugin),
                        new Death(Plugin),
                        new Health(Plugin),
                        new Wipe(Plugin)
                    };

                    RegisterGameEvents();
                } else
                {
                    throw new FileNotFoundException("Unable to get coreProps. Is SteelSeries GG installed?");
                }
            }
        }

        public async void RegisterGame()
        {
            var data = new Dictionary<string, string>
            {
                { "game", this.Game },
                { "game_display_name", this.GameDisplayName },
                { "developer", this.Developer }
            };

            await Post("game_metadata", data);

            TimeSpan delay = TimeSpan.FromMilliseconds(this.HeartbeatDelay);
            this.heartbeatTimer = new Timer(Heartbeat, null, delay, delay);
        }

        /// <summary>
        /// Sends a heartbeat packet
        /// </summary>
        public async void Heartbeat(object? state)
        {
            var data = new { game = this.Game };
            await Post("game_heartbeat", data);
        }

        public async void RegisterAutoclips()
        {
            List<AutoclipRule> clipRules = new List<AutoclipRule>();
            foreach (IGameEvent gameEvent in GameEvents)
            {
                if (gameEvent is IAutoClipEvent rule)
                {
                    clipRules.Add(new AutoclipRule(rule.Name, rule.Label, rule.Enabled));
                }
            }

            var data = new
            {
                game = this.Game,
                rules = clipRules.ToArray()
            };
            await Post("register_autoclip_rules", data);
        }

        internal async void RegisterGameEvents()
        {
            List<AutoclipRule> clipRules = new List<AutoclipRule>();
            List<object> timelineList = new List<object>();

            foreach (IGameEvent gameEvent in GameEvents)
            {
                gameEvent.UnsubscribeFromEvents();
                gameEvent.SubscribeToEvents();

                if (gameEvent is IAutoClipEvent rule)
                {
                    clipRules.Add(new AutoclipRule(rule.Name, rule.Label, rule.Enabled));
                }

                if (gameEvent is ITimelineEvent tlEvent)
                {
                    timelineList.Add(new
                    {
                        @event = tlEvent.Name,
                        icon_id = tlEvent.TimelineIconId,
                        previewable = tlEvent.Previewable
                    });
                }

                var data = new
                {
                    game = this.Game,
                    @event = gameEvent.Name,
                    min_value = gameEvent.MinValue,
                    max_value = gameEvent.MaxValue,
                    icon_id = gameEvent.IconId,
                    value_optional = gameEvent.ValueOptional
                };

                await Post("register_game_event", data);
            }

            var clipData = new
            {
                game = this.Game,
                rules = clipRules.ToArray()
            };

            await Post("register_autoclip_rules", clipData);

            var timelineData = new
            {
                game = this.Game,
                events = timelineList.ToArray()
            };

            await Post("register_timeline_events", timelineData);
        }

        //private async void RegisterTimelineEvents()
        //{
        //    List<object> eventList = new List<object>();

        //    foreach (ITimelineEvent timelineEvent in TimelineEvents)
        //    {
        //        eventList.Add(new
        //        {
        //            @event = timelineEvent.Name,
        //            icon_id = timelineEvent.IconID,
        //            previewable = timelineEvent.Previewable
        //        });
        //    }

        //    var data = new
        //    {
        //        game = this.Game,
        //        events = eventList.ToArray()
        //    };

        //    await Post("register_timeline_events", data);
        //}

        /// <summary>
        /// Triggers an Autoclip event
        /// </summary>
        /// <param name="key">The autoclip rule to trigger</param>
        internal async void Autoclip(IAutoClipEvent rule)
        {
            // If it's been less than 10 seconds, dont clip.
            if ((DateTime.Now - lastClip).TotalSeconds < 10)
            {
                lastClip = DateTime.Now;
                return;
            }

            var data = new Dictionary<string, string>
            {
                { "game", this.Game },
                { "key", rule.Name }
            };

            if (Plugin.Configuration.LogAutoclipsToChat)
            {
                Plugin.ChatGui.Print($"[GameSense] Autoclipping {rule.Label}.");
            }

            lastClip = DateTime.Now;
            await Post("autoclip", data);
        }

        /// <summary>
        /// Trigger a game event for GameSense
        /// </summary>
        internal async void SendGameEvent(IGameEvent gameEvent, int eventValue)
        {
            var data = new
            {
                game = this.Game,
                @event = gameEvent.Name,
                data = new {
                    value = eventValue
                }
            };

            await Post("game_event", data);
        }

        internal async void SendGameEvent(IGameEvent gameEvent)
        {
            var data = new
            {
                game = this.Game,
                @event = gameEvent.Name,
                data = new {}
            };

            await Post("game_event", data);
        }

        /// <summary>
        /// Sends a POST request to the GameSense API
        /// </summary>
        /// <param name="path">The path to send the request to</param>
        /// <param name="data">The data to embed</param>
        /// <returns></returns>
        public async Task<string> Post(string path, object data)
        {
            // Stringify the data
            using StringContent jsonContent = new(JsonConvert.SerializeObject(data, Formatting.Indented), Encoding.UTF8, "application/json");
            string request = await jsonContent.ReadAsStringAsync();
            Plugin.Log.Verbose(request);

            // Send the POST request
            using HttpResponseMessage response = await httpClient.PostAsync(path, jsonContent);

            // Await the response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Plugin.Log.Verbose(jsonResponse);

            // Return the response
            return jsonResponse;
        }

        // -------- Wrapper classes --------

        public class AutoclipRule
        {
            public string rule_key { get; init; }
            public string label { get; init; }
            public bool enabled { get; set; }

            public AutoclipRule(string ruleKey, string ruleLabel, bool enabled = true)
            {
                this.rule_key = ruleKey;
                this.label = ruleLabel;
                this.enabled = enabled;
            }
        }

        private class CoreProps
        {
            public string address { get; set; }
        }
    }
}
