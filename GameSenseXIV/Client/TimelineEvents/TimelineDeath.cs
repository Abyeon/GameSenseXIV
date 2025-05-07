using GameSenseXIV.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.TimelineEvents
{
    internal class TimelineDeath : ITimelineEvent
    {
        public string Name => "DEATH";
        public string IconID => "DEATH";
        public int Previewable => 1;
    }
}
