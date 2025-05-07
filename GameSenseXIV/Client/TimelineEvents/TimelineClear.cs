using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.TimelineEvents
{
    internal class TimelineClear : ITimelineEvent
    {
        public string Name => "CLEAR";
        public string IconID => "WIN";
        public int Previewable => 1;
    }
}
