using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Services
{
    internal interface IAutoClipRule : IDisposable
    {
        string RuleKey { get; }
        string Label { get; }
        string Description { get; }
        bool Enabled { get; set; }

        /// <summary>
        /// Subscribe to any events
        /// </summary>
        void SubscribeToEvents();

        /// <summary>
        /// Unsubscribes from all events
        /// </summary>
        void UnsubscribeFromEvents();

        void Toggle()
        {
            this.Enabled ^= true; // Flip
            Setup();
        }

        void Setup()
        {
            if (!this.Enabled)
            {
                Plugin.Log.Debug($"{this.Label}: Unsubscribing from events.");
                UnsubscribeFromEvents();
            }
            else
            {
                Plugin.Log.Debug($"{this.Label}: Subscribing to events.");
                SubscribeToEvents();
            }
        }
    }
}
