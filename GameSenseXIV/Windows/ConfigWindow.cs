using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using GameSenseXIV.Client;
using GameSenseXIV.Services;
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
            MinimumSize = new Vector2(232, 140),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw() { }

    public override void Draw()
    {
        ImGui.TextUnformatted("Delay till next clip:");

        using (ImRaii.ItemWidth(25f))
        {
            int delayMinutes = Configuration.DelayMinutes;
            if (ImGui.DragInt("minutes", ref delayMinutes, 0.1f, 0, 5))
            {
                Configuration.DelayMinutes = delayMinutes;
                Configuration.Save();
            }

            ImGui.SameLine();

            int delaySeconds = Configuration.DelaySeconds;
            if (ImGui.DragInt("seconds", ref delaySeconds, 0.1f, 0, 5))
            {
                Configuration.DelaySeconds = delaySeconds;
                Configuration.Save();
            }
        }

        bool clipAfter = Configuration.ClipAfterDelay;
        if (ImGui.Checkbox("Clip after delay", ref clipAfter))
        {
            Configuration.ClipAfterDelay = clipAfter;
            Configuration.Save();
        }

        ImGui.TextUnformatted("Autoclip Rules: ");

        foreach (IGameEvent gameEvent in Plugin.GSClient.GameEvents)
        {
            if (gameEvent is IAutoClipEvent rule)
            {
                bool enabled = rule.Enabled;
                if (ImGui.Checkbox(rule.Label, ref enabled))
                {
                    rule.Toggle();
                    Plugin.GSClient.RegisterAutoclips();
                    Configuration.Save();
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip(rule.Description);
                }
            }
        }

        bool autoclipChat = Configuration.LogAutoclipsToChat;
        if (ImGui.Checkbox("Log autoclipping to chat", ref autoclipChat))
        {
            Configuration.LogAutoclipsToChat = autoclipChat;
            Configuration.Save();
        }
    }
}
