using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using GameSenseXIV.Windows;
using GameSenseXIV.Services;
using System;
using System.Threading;
using Dalamud.Game.ClientState.Objects.SubKinds;
using System.Collections.Generic;
using System.Linq;

namespace GameSenseXIV;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IFramework Framework {  get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IPartyList PartyList { get; private set; } = null!;

    internal static GameSense GSClient { get; private set; } = null!;

    private const string CommandName = "/pmycommand";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("GameSenseXIV");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        GSClient = new GameSense("FFXIV", "Final Fantasy XIV Online", "Square Enix", 14000);

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        Framework.Update += OnFrameworkUpdate;

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    private List<uint> deadPlayerIds = new List<uint>(); 

    // Check if a player died
    private void HandlePlayerHealth(IPlayerCharacter character)
    {
        if (character == null) return;

        if (character.CurrentHp == 0)
        {
            // Player just died
            if (!deadPlayerIds.Any(x => x == character.DataId))
            {
                deadPlayerIds.Add(character.DataId);

                // Party / alliance wipe
                if (deadPlayerIds.Count == PartyList.Count)
                {
                    GSClient.Autoclip("wipe");
                } else if (character.DataId == ClientState.LocalPlayer.DataId) // LocalPlayer death
                {
                    GSClient.Autoclip("death");
                }
            }
        } else if (deadPlayerIds.Any(x => x == character.DataId))
        {
            // Player revived
            deadPlayerIds.Remove(character.DataId);
        }
    }

    // Run every frame
    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!ClientState.IsLoggedIn || ClientState.LocalPlayer == null) return;

        if (PartyList == null || PartyList.Count == 0)
        {
            HandlePlayerHealth(ClientState.LocalPlayer);
        } else
        {
            foreach (IPlayerCharacter character in PartyList)
            {
                HandlePlayerHealth(character);
            }
        }
    }

    public void Dispose()
    {
        GSClient.Dispose();

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        GSClient.Autoclip("death");
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
