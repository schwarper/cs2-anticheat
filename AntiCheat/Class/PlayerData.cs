using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiCheat.Class;

public class PlayerData
{
    private static readonly Dictionary<ulong, PlayerData> _playerData = [];

    public AntiDLLData AntiDLL { get; } = new();
    public BunnyHopData BunnyHop { get; } = new();
    public SilentAimData SilentAim { get; } = new();
    public SpinbotData Spinbot { get; } = new();
    public WallhackData Wallhack { get; } = new();
    public TeleportData Teleport { get; } = new();
    public RapidFireData RapidFire { get; } = new();

    public static PlayerData Get(CCSPlayerController player)
    {
        if (!_playerData.TryGetValue(player.SteamID, out PlayerData? playerData))
        {
            playerData = new PlayerData();
            _playerData[player.SteamID] = playerData;
        }

        return playerData;
    }

    public static void Set(CCSPlayerController player)
    {
        _playerData[player.SteamID] = new PlayerData();
    }

    public static void Remove(CCSPlayerController player)
    {
        _playerData.Remove(player.SteamID);
    }
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