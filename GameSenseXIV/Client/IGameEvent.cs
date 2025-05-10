using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client
{
    internal interface IGameEvent : IDisposable
    {
        /// <summary>
        /// Limited to uppercase A-Z, 0-9, hyphen, and underscore characters.
        /// </summary>
        string Name { get; }
        string Label { get; }
        string Description { get; }
        int MinValue { get; }
        int MaxValue { get; }
        int IconId { get; }
        bool ValueOptional { get; }
        object[]? Handlers => null;

        /// <summary>
        /// Subscribe to any events
        /// </summary>
        void SubscribeToEvents();

        /// <summary>
        /// Unsubscribes from all events
        /// </summary>
        void UnsubscribeFromEvents();
    }
}
