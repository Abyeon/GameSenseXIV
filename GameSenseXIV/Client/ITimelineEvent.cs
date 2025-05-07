using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client
{
    internal interface ITimelineEvent
    {
        /// <summary>
        /// Same as the GameEvent name
        /// </summary>
        string Name { get; }

        /// <see cref="https://github.com/SteelSeries/gamesense-sdk/blob/master/doc/api/sending-moments-events.md#timeline-icons"/>
        string IconID { get; }

        /// <summary>
        /// Whether the event should show up in the clip thumbnail
        /// </summary>
        int Previewable { get; }
    }
}
