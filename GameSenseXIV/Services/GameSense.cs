using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GameSenseXIV.Services
{
    public class GameSense : IDisposable
    {
        string Game { get; init; }
        string GameDisplayName {  get; init; }
        string Developer { get; init; }
        uint HeartbeatDelay { get; init; }

        public List<AutoclipRule> AutoclipRules = new List<AutoclipRule>();

        private Uri Address { get; init; }
        private HttpClient httpClient { get; set; }
        private Timer heartbeatTimer { get; set; }

        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient?.Dispose();
            }

            heartbeatTimer.Dispose();
        }

        public GameSense(string game, string gameDisplayName, string developer, uint heartbeatDelay = 14000)
        {
            this.Game = game;
            this.GameDisplayName = gameDisplayName;
            this.Developer = developer;
            this.HeartbeatDelay = heartbeatDelay;

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

                    // Add Autoclip rules
                    AutoclipRules.Add(new AutoclipRule("death", "Player death", true));
                    AutoclipRules.Add(new AutoclipRule("wipe", "Party wipe", true));
                    AutoclipRules.Add(new AutoclipRule("duty_complete", "Duty Completion", true));

                    // Register them
                    RegisterAutoclip();
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

        /// <summary>
        /// Register Autoclip Events
        /// </summary>
        public async void RegisterAutoclip()
        {
            var data = new {
                game = this.Game,
                rules = this.AutoclipRules.ToArray()
            };
            await Post("register_autoclip_rules", data);
        }

        /// <summary>
        /// Triggers an Autoclip event
        /// </summary>
        /// <param name="key">The key of the autoclip rule to trigger</param>
        public async void Autoclip(string key)
        {
            var data = new Dictionary<string, string>
            {
                { "game", this.Game },
                { "key", key }
            };

            Plugin.ChatGui.Print($"[GameSense] Autoclipping {key.ToLower()}.");

            await Post("autoclip", data);
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
            Plugin.Log.Debug(request);

            // Send the POST request
            using HttpResponseMessage response = await httpClient.PostAsync(path, jsonContent);

            // Await the response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Plugin.Log.Debug(jsonResponse);

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

        public class TimelineEvent
        {

        }

        private class CoreProps
        {
            public string address { get; set; }
        }
    }
}
