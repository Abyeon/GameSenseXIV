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
        bool Enabled { get; }

        /// <summary>
        /// Subscribe to any events in the Plugin
        /// </summary>
        void SubscribeToEvents();
    }
}
