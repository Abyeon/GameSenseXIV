using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace GameSenseXIV;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool LogAutoclipsToChat = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
