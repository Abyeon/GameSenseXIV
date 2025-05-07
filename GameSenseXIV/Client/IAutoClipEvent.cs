using GameSenseXIV.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Services
{
    internal interface IAutoClipEvent : ITimelineEvent
    {
        bool Enabled { get; set; }

        void Toggle()
        {
            this.Enabled ^= true; // Flip
        }
    }
}
