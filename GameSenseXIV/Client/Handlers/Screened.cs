using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSenseXIV.Client.Handlers
{
    internal class Screened
    {
        [JsonProperty("device-type")]
        public static string DeviceType = "screened";

        [JsonProperty("mode")]
        public static string Mode = "screen";

        [JsonProperty("datas")]
        public object[] Datas = null;
    }
}
