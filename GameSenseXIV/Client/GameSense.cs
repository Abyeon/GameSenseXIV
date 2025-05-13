using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameSenseXIV.Client;
using GameSenseXIV.Client.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static GameSenseXIV.Client.Wrappers;

namespace GameSenseXIV.Services
{
    public class GameSense : IDisposable
    {
        string Game { get; init; }
        string GameDisplayName {  get; init; }
        string Developer { get; init; }
        uint HeartbeatDelay { get; init; }

        internal List<IGameEvent> GameEvents = new List<IGameEvent>();

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

                    // Initiate the Http client
                    httpClient = new HttpClient()
                    {
                        BaseAddress = this.Address
                    };

                    // Register game
                    RegisterGame();

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
            var data = new GameMetadata(this.Game, this.GameDisplayName, this.Developer);

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

                var data = new RegisteredGameEvent(this.Game, gameEvent.Name, gameEvent.MinValue, gameEvent.MaxValue, gameEvent.IconId, gameEvent.ValueOptional, gameEvent.Handlers);

                if (data.Handlers == null)
                {
                    await Post("register_game_event", data);
                } else
                {
                    await Post("bind_game_event", data);
                }
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

        /// <summary>
        /// Triggers an Autoclip event
        /// </summary>
        /// <param name="key">The autoclip rule to trigger</param>
        internal async void Autoclip(IAutoClipEvent rule)
        {
            // If it has been less than 10 seconds, dont clip.
            int minutes = Plugin.Configuration.DelayMinutes;
            int seconds = Plugin.Configuration.DelaySeconds;

            if ((DateTime.Now - lastClip).TotalSeconds < ((60 * minutes) + seconds))
            {
                lastClip = DateTime.Now;
                return;
            }

            var data = new
            {
                game = this.Game,
                key = rule.Name
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
        internal async void SendGameEvent(IGameEvent gameEvent)
        {
            var data = new
            {
                game = this.Game,
                @event = gameEvent.Name,
                data = new { }
            };

            await Post("game_event", data);
        }

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

        internal async void SendGameEvent(IGameEvent gameEvent, object eventData)
        {
            var data = new
            {
                game = this.Game,
                @event = gameEvent.Name,
                data = eventData
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
            try
            {
                // Create settings
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };

                // Stringify the data
                using StringContent jsonContent = new(JsonConvert.SerializeObject(data, Formatting.Indented), Encoding.UTF8, "application/json");
                string request = await jsonContent.ReadAsStringAsync();
                Plugin.Log.Verbose(request);

                // Send the POST request
                using HttpResponseMessage response = await httpClient.PostAsync(path, jsonContent);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Handle possible errors
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    dynamic json = JValue.Parse(jsonResponse);
                    string error = json.error;

                    Plugin.ChatGui.PrintError($"[GameSense] Error, please check /xllog");

                    if (error != null && error != String.Empty)
                    {
                        Plugin.Log.Error($"Error sending data to path /\"{path}\":\n{request}");
                        Plugin.Log.Error(error);
                    }
                }

                // Print the response
                Plugin.Log.Verbose(jsonResponse);

                // Return it
                return jsonResponse;
            } catch (Exception ex)
            {
                Plugin.ChatGui.PrintError("[GameSense] Fatal error, please check /xllog." +
                    "\nYou may need to restart the plugin for it to work again.");

                Plugin.Log.Error($"Error: {ex.Message}\n{ex.ToString()}");

                return ex.Message;
            }
        }

        // -------- Wrapper classes --------

        private class CoreProps
        {
            public string address { get; set; }
        }
    }
}
