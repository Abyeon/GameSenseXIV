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
using Dalamud.Game.ClientState.Party;

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
    [PluginService] internal static IDutyState DutyState { get; private set; } = null!;

    internal static GameSense GSClient { get; private set; } = null!;

    public Configuration Configuration { get; init; }
    public readonly WindowSystem WindowSystem = new("GameSenseXIV");

    // Events
    public event EventHandler<uint> OnHealthChanged;
    public event EventHandler OnDeath;

    private const string CommandName = "/gamesense";
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        GSClient = new GameSense(this, "FFXIV", "Final Fantasy XIV Online", "Square Enix", 14000);

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the gamesense config."
        });

        Framework.Update += OnFrameworkUpdate;
        //DutyState.DutyWiped += OnDutyWipe;
        //DutyState.DutyCompleted += OnDutyComplete;

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
    }

    private uint lastHP;

    // Handle the player health updating
    private void HandlePlayerHealth(IPlayerCharacter character)
    {
        if (character.CurrentHp != lastHP)
        {
            if (character.CurrentHp == 0)
            {
                OnDeath?.Invoke(this, EventArgs.Empty);
            }

            OnHealthChanged?.Invoke(character, character.CurrentHp);

            lastHP = character.CurrentHp;
        }
    }

    // Run every frame
    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!ClientState.IsLoggedIn || ClientState.LocalPlayer == null) return;

        HandlePlayerHealth(ClientState.LocalPlayer);
    }

    public void Dispose()
    {
        GSClient.Dispose();

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private async void OnCommand(string command, string args)
    {
        ToggleConfigUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
