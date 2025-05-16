using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace GameSenseXIV;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int DelayMinutes = 0;
    public int DelaySeconds = 30;

    public bool ClipAfterDelay = true;

    public bool LogAutoclipsToChat = true;
    public bool ClipDeaths = false;
    public bool ClipWipes = true;
    public bool ClipClears = true;
    public bool ClipPvpKills = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
