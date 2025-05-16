using GameSenseXIV.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Common.Lua;

namespace GameSenseXIV.Client.Events
{
    internal class PvpKill : IAutoClipEvent
    {
        public string Name => "KILL";
        public string Label => "Pvp Kill";
        public string Description => "Triggers when you kill a player in PVP.";
        public int MinValue => 0;
        public int MaxValue => 100;
        public int IconId => (int)Icons.EventIcon.Dead;
        public string TimelineIconId => "KILL";
        public int Previewable => 1;
        public bool ValueOptional => true;
        public bool Enabled
        {
            get { return Plugin.Configuration.ClipPvpKills; }
            set { Plugin.Configuration.ClipPvpKills = value; }
        }

        private Plugin Plugin { get; set; }

        public PvpKill(Plugin plugin)
        {
            this.Plugin = plugin;
        }

        public void Dispose() { }

        public void SubscribeToEvents()
        {
            Plugin.ChatGui.ChatMessage += OnChatMessage;
        }

        public void UnsubscribeFromEvents()
        {
            Plugin.ChatGui.ChatMessage -= OnChatMessage;
        }

        private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!Plugin.ClientState.IsPvP) return;
            if (!message.TextValue.Contains("You defeat")) return;

            if ((ushort)type == (ushort)2361 || (ushort)type == (ushort)2874)
            {
                Plugin.ChatGui.Print("[GameSense] Kill!");
                Plugin.Log.Debug("Detected defeat message");
                Plugin.GSClient.SendGameEvent(this);

                if (Enabled)
                {
                    Plugin.GSClient.Autoclip(this);
                }
            } else
            {
                Plugin.Log.Debug("Possible mismatch of types: ");
                string name = Enum.GetName(typeof(XivChatType), type);
                if (name != null)
                {
                    Plugin.Log.Debug(name);
                }
                else
                {
                    Plugin.Log.Debug("NAME WAS NULL " + (ushort)type);
                }
            }
        }
    }
}
