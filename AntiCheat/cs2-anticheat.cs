using System.Text;
using AntiCheat.Class;
using AntiCheat.Enum;
using AntiCheat.Interface;
using AntiCheat.Modules.AntiDLL;
using AntiCheat.Modules.RapidFire;
using AntiCheat.Modules.Scroll;
using AntiCheat.Modules.SilentAim;
using AntiCheat.Modules.Spinbot;
using AntiCheat.Modules.Teleport;
using AntiCheat.Modules.Wallhack;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.ValveConstants.Protobuf;
using static AntiCheat.Class.Hook_ProcessUsercmds;

namespace AntiCheat;

public class AntiCheat : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "AntiCheat";
    public override string ModuleVersion => "1.10";
    public override string ModuleAuthor => "schwarper";

    public static AntiCheat Instance { get; private set; } = new();
    public Config Config { get; set; } = new();
    public ResultType ResultType { get; private set; }

    private readonly Dictionary<CheatType, ICheatDetector> _detectors = [];

    public override void Load(bool hotReload)
    {
        Instance = this;

        foreach (ICheatDetector detector in _detectors.Values)
        {
            detector.Load();
        }

        if (_detectors.Values.Any(d => d.RequiresProcessUsercmdsHook))
        {
            ProcessUsercmds.Hook(OnProcessUsercmds, HookMode.Post);
        }

        if (hotReload)
        {
            List<CCSPlayerController> players = Utilities.GetPlayers();
            foreach (CCSPlayerController player in players)
            {
                if (player.IsBot)
                    continue;

                PlayerData.Set(player);
            }
        }
    }

    public override void Unload(bool hotReload)
    {
        foreach (ICheatDetector detector in _detectors.Values)
        {
            detector.Unload();
        }

        if (_detectors.Values.Any(d => d.RequiresProcessUsercmdsHook))
        {
            ProcessUsercmds.Unhook(OnProcessUsercmds, HookMode.Post);
        }
    }

    public HookResult OnProcessUsercmds(DynamicHook hook)
    {
        CCSPlayerController player = hook.GetParam<CCSPlayerController>(0);
        CUserCmd userCmd = new(hook.GetParam<IntPtr>(1));
        QAngle? angle = userCmd.GetViewAngles();

        if (angle == null)
            return HookResult.Continue;

        foreach (ICheatDetector? detector in _detectors.Values.Where(d => d.RequiresProcessUsercmdsHook))
        {
            detector.OnProcessUsercmds(player, new QAngle(angle.X, angle.Y, angle.Z));
        }

        return HookResult.Continue;
    }

    public void OnConfigParsed(Config config)
    {
        config.Tag = config.Tag.ReplaceColorTags();

        _detectors.Clear();

        if (config.Modules.AntiDLL.Enabled) _detectors[CheatType.Event] = new AntiDLL();
        if (config.Modules.RapidFire.Enabled) _detectors[CheatType.RapidFire] = new RapidFire();
        if (config.Modules.Scroll.Enabled) _detectors[CheatType.Scroll] = new Scroll();
        if (config.Modules.SilentAim.Enabled) _detectors[CheatType.SilentAim] = new SilentAim();
        if (config.Modules.Spinbot.Enabled) _detectors[CheatType.Spinbot] = new Spinbot();
        if (config.Modules.Teleport.Enabled) _detectors[CheatType.Teleport] = new Teleport();
        if (config.Modules.Wallhack.Enabled) _detectors[CheatType.Wallhack] = new Wallhack();

        ResultType = config.Type switch
        {
            "PrintAll" => ResultType.PrintAll,
            "PrintAdmin" => ResultType.PrintAdmin,
            "Kick" => ResultType.Kick,
            "Ban" => ResultType.Ban,
            _ => ResultType.PrintAdmin
        };

        Config = config;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo _)
    {
        if (@event.Userid is not { } player || player.IsBot)
            return HookResult.Continue;

        PlayerData.Set(player);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo _)
    {
        if (@event.Userid is not { } player || player.IsBot)
            return HookResult.Continue;

        PlayerData.Remove(player);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo _)
    {
        if (@event.Userid is not CCSPlayerController victim ||
            @event.Attacker is not CCSPlayerController attacker ||
            victim == attacker)
            return HookResult.Continue;

        if (_detectors.TryGetValue(CheatType.Spinbot, out ICheatDetector? spinbotDetector))
            spinbotDetector.OnPlayerDeath(victim, attacker);
        if (_detectors.TryGetValue(CheatType.SilentAim, out ICheatDetector? silentDetector))
            silentDetector.OnPlayerDeath(victim, attacker);

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo _)
    {
        if (@event.Userid is not CCSPlayerController player)
            return HookResult.Continue;

        if (_detectors.TryGetValue(CheatType.RapidFire, out ICheatDetector? rapidDetector))
            rapidDetector.OnWeaponFire(player);

        return HookResult.Continue;
    }

    public void OnPlayerDetected(CCSPlayerController player, CheatType cheatType, string detail = "")
    {
        if (!string.IsNullOrEmpty(detail))
        {
            detail = $" ({detail})";
        }

        if (!string.IsNullOrWhiteSpace(Config.DiscordWebhook))
        {
            StringBuilder messageBuilder = new StringBuilder()
                .AppendLine("```")
                .AppendLine($"Server IP: {ServerIP.Get()}")
                .AppendLine($"Player: {player.PlayerName}")
                .AppendLine($"SteamID: {player.SteamID}")
                .AppendLine($"Cheat Type: {cheatType}");

            if (!string.IsNullOrWhiteSpace(detail))
            {
                messageBuilder.AppendLine($"Details: {detail}");
            }

            messageBuilder.AppendLine("```");

            _ = Task.Run(() => DiscordNotifier.SendDiscordAsync(messageBuilder.ToString()));
        }

        Server.NextWorldUpdate(() =>
        {
            Microsoft.Extensions.Localization.LocalizedString reason = Instance.Localizer["Suspicious behavior detected", player.PlayerName, cheatType, detail];

            switch (ResultType)
            {
                case ResultType.PrintAll:
                    PrintToChatAll(player.PlayerName, cheatType, detail, false);
                    break;
                case ResultType.PrintAdmin:
                    PrintToChatAll(player.PlayerName, cheatType, detail, true);
                    break;
                case ResultType.Kick:
                    player.Disconnect(NetworkDisconnectionReason.NETWORK_DISCONNECT_KICKED_VACNETABNORMALBEHAVIOR);
                    break;
                case ResultType.Ban:
                    Server.ExecuteCommand($"mm_ban {player.UserId} {Config.Time} `{reason}`");
                    Server.ExecuteCommand($"css_ban {player.UserId} {Config.Time} `{reason}`");
                    break;
            }
        });
    }

    public static void PrintToChatAll(string playername, CheatType cheatType, string detail, bool onlyAdmin = false)
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();
        foreach (CCSPlayerController player in players)
        {
            if (player.IsBot)
                continue;

            if (onlyAdmin && !AdminManager.PlayerHasPermissions(player, "@css/ban"))
                continue;

            player.PrintToChat(Instance.Config.Tag + Instance.Localizer.ForPlayer(player, "Suspicious behavior detected", playername, cheatType, detail));
        }
    }
}