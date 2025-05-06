using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace GameSenseXIV.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("GameSense Config###GameSenseXIV")
    {
        Flags = ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(232, 90),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw() { }

    public override void Draw()
    {
        bool autoclipChat = Configuration.LogAutoclipsToChat;
        if (ImGui.Checkbox("Log Autoclipping to Chat", ref autoclipChat))
        {
            Configuration.LogAutoclipsToChat = autoclipChat;
            Configuration.Save();
        }

        bool clipDeaths = Configuration.ClipDeaths;
        if (ImGui.Checkbox("Autoclip Deaths", ref clipDeaths))
        {
            Configuration.ClipDeaths = clipDeaths;
            Configuration.Save();
        }

        bool clipWipes = Configuration.ClipWipes;
        if (ImGui.Checkbox("Autoclip Party Wipes", ref clipWipes))
        {
            Configuration.ClipWipes = clipWipes;
            Configuration.Save();
        }

        bool clipClears = Configuration.ClipClears;
        if (ImGui.Checkbox("Autoclip Duty Completion", ref clipClears))
        {
            Configuration.ClipClears = clipClears;
            Configuration.Save();
        }
    }
}
