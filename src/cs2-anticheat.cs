using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.ValveConstants.Protobuf;
using CS2_SimpleAdminApi;
using System.Runtime.InteropServices;
using System.Text;
using static AntiCheat.Hook_ProcessUsercmds;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;
using CounterStrikeSharp.API.Modules.Entities;

namespace AntiCheat;

public class AntiCheat : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "AntiCheat";
    public override string ModuleVersion => "1.3";
    public override string ModuleAuthor => "schwarper";

    public static AntiCheat Instance { get; set; } = new();
    public Config Config { get; set; } = new Config();
    public ResultType ResultType { get; set; }
    private readonly Dictionary<ulong, PlayerData> _playerData = [];
    private readonly Dictionary<CheatType, ICheatDetector> _detectors = [];
    private ICS2_SimpleAdminApi? _simpleAdminAPi;

    private readonly HashSet<CheatType> CheatTypesOnProcessUsercmds = [
        CheatType.Scroll,
        CheatType.SilentAim,
        CheatType.Spinbot,
        CheatType.Teleport,
        CheatType.Wallhack,
    ];

    public override void Load(bool hotReload)
    {
        Instance = this;

        foreach (ICheatDetector detector in _detectors.Values)
        {
            detector.Load();
        }

        foreach (CheatType cheatType in CheatTypesOnProcessUsercmds)
        {
            if (_detectors.TryGetValue(cheatType, out ICheatDetector? detector))
            {
                ProcessUsercmds.Hook(OnProcessUsercmds, HookMode.Post);
                break;
            }
        }

        if (hotReload)
        {
            List<CCSPlayerController> players = Utilities.GetPlayers();
            foreach (CCSPlayerController player in players)
            {
                _playerData[player.SteamID] = new PlayerData();
            }
        }
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        try
        {
            _simpleAdminAPi = ICS2_SimpleAdminApi.PluginCapability.Get();
        }
        catch (Exception)
        {
            
        }
    }

    public override void Unload(bool hotReload)
    {
        foreach (ICheatDetector detector in _detectors.Values)
        {
            detector.Unload();
        }

        foreach (CheatType cheatType in CheatTypesOnProcessUsercmds)
        {
            if (_detectors.TryGetValue(cheatType, out ICheatDetector? detector))
            {
                ProcessUsercmds.Unhook(OnProcessUsercmds, HookMode.Post);
                break;
            }
        }
    }

    public void OnConfigParsed(Config config)
    {
        Config = config;

        Config.Tag = Config.Tag.ReplaceColorTags();

        _detectors.Clear();

        if (Config.Modules.AntiDLL.Enabled)
            _detectors[CheatType.Event] = new AntiDLL();

        if (Config.Modules.RapidFire.Enabled)
            _detectors[CheatType.RapidFire] = new RapidFireDetector();

        if (Config.Modules.BunnyHop.Enabled)
            _detectors[CheatType.Scroll] = new BunnyHopDetector();

        if (Config.Modules.SilentAim.Enabled)
            _detectors[CheatType.SilentAim] = new SilentAimDetector();

        if (Config.Modules.Spinbot.Enabled)
            _detectors[CheatType.Spinbot] = new SpinbotDetector();

        if (Config.Modules.Teleport.Enabled)
            _detectors[CheatType.Teleport] = new TeleportDetector();

        if (Config.Modules.Wallhack.Enabled)
            _detectors[CheatType.Wallhack] = new WallhackDetector();

        ResultType = Config.Type switch
        {
            "PrintAll" => ResultType.PrintAll,
            "PrintAdmin" => ResultType.PrintAdmin,
            "Kick" => ResultType.Kick,
            "Ban" => ResultType.Ban,
            _ => ResultType.PrintAdmin
        };
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid is not { } player)
            return HookResult.Continue;

        _playerData[player.SteamID] = new PlayerData();
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid is not { } player)
            return HookResult.Continue;

        _playerData.Remove(player.SteamID);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
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
    public HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player)
            return HookResult.Continue;

        if (_detectors.TryGetValue(CheatType.RapidFire, out ICheatDetector? rapidDetector))
            rapidDetector.OnWeaponFire(player);

        return HookResult.Continue;
    }

    public HookResult OnProcessUsercmds(DynamicHook hook)
    {
        CCSPlayerController player = hook.GetParam<CCSPlayerController>(0);
        CUserCmd userCmd = new(hook.GetParam<IntPtr>(1));
        QAngle? angle = userCmd.GetViewAngles();

        if (angle == null)
            return HookResult.Continue;

        foreach (CheatType cheatType in CheatTypesOnProcessUsercmds)
        {
            if (_detectors.TryGetValue(cheatType, out ICheatDetector? detector))
            {
                if (cheatType == CheatType.Teleport)
                    detector.OnProcessUsercmds(player, angle);
                else
                    detector.OnProcessUsercmds(player, new QAngle(angle.X, angle.Y, angle.Z));
            }
        }

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
                .AppendLine($"Server IP: {GetServerIP()}")
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
            var reason = Instance.Localizer["Suspicious behavior detected", player.PlayerName, cheatType, detail];
            
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
                    Server.ExecuteCommand($"mm_ban {player.UserId} 0 `{reason}`");
                    _simpleAdminAPi?.IssuePenalty(new SteamID(player.SteamID), null, PenaltyType.Ban, reason, -1);
                    break;
            }
        });
    }

    public PlayerData? GetPlayerData(CCSPlayerController player)
    {
        return _playerData.TryGetValue(player.SteamID, out PlayerData? data) ? data : null;
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

    private delegate nint CNetworkSystem_UpdatePublicIp(nint a1);
    private CNetworkSystem_UpdatePublicIp? _networkSystemUpdatePublicIp;

    private string GetServerIP()
    {
        nint _networkSystem = NativeAPI.GetValveInterface(0, "NetworkSystemVersion001");

        unsafe
        {
            if (_networkSystemUpdatePublicIp == null)
            {
                nint funcPtr = *(nint*)(*(nint*)(_networkSystem) + 256);
                _networkSystemUpdatePublicIp = Marshal.GetDelegateForFunctionPointer<CNetworkSystem_UpdatePublicIp>(funcPtr);
            }

            byte* ipBytes = (byte*)(_networkSystemUpdatePublicIp(_networkSystem) + 4);
            string ip = $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}.{ipBytes[3]}";

            return ip;
        }
    }
}

public enum ResultType
{
    PrintAll,
    PrintAdmin,
    Kick,
    Ban
}

public enum CheatType
{
    Event,
    Scroll,
    SilentAim,
    Spinbot,
    Wallhack,
    RapidFire,
    Teleport
}

public class PlayerData
{
    public AntiDLLData AntiDLL { get; } = new();
    public BunnyHopData BunnyHop { get; } = new();
    public SilentAimData SilentAim { get; } = new();
    public SpinbotData Spinbot { get; } = new();
    public WallhackData Wallhack { get; } = new();
    public TeleportData Teleport { get; } = new();
    public RapidFireData RapidFire { get; } = new();
}

public class AntiDLLData
{
    public double LastTickCount;
}

public class BunnyHopData
{
    public List<JumpStats> JumpStats { get; } = [];
    public int GroundTicks { get; set; }
    public int ReleaseTick { get; set; }
    public int AirTicks { get; set; }
    public bool PreviousGround { get; set; } = true;
    public PlayerButtons PreviousButtons { get; set; }
    public int CurrentJump { get; set; }
    public int[] TempStats { get; } = new int[5];
}

public class JumpStats
{
    public int Scrolls { get; set; }
    public int BeforeGround { get; set; }
    public int AfterGround { get; set; }
    public int AverageTicks { get; set; }
    public bool PerfectJump { get; set; }
}

public class SilentAimData
{
    public CCSPlayerController? Victim;
    public bool RecentlyKilled;
    public int SuspicionCount;
}

public class SpinbotData
{
    public QAngle LastAngle = new();
    public double LastTickCount;
    public bool RecentlyKilled;
    public int SuspicionCount;
}

public class WallhackData
{
    public QAngle? LastAngle;
    public Vector? Mins = new();
    public Vector? Maxs = new();
}

public class TeleportData
{
    public double LastTickCount;
    public int SuspicionCount;
}

public class RapidFireData
{
    public int LastShotTick;
    public int SuspicionCount;
}

public enum State
{
    Nothing,
    Landing,
    Jumping,
    Pressing,
    Releasing
}

public enum Stats
{
    Scrolls,
    BeforeGround,
    AfterGround,
    AverageTicks,
    PerfectJump,
    Size
}
