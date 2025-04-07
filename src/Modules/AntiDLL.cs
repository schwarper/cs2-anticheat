using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using static AntiCheat.AntiCheat;

namespace AntiCheat;

public class AntiDLL : ICheatDetector
{
    private IGameEventManager2? _gameEventManager;

    public void Load()
    {
        IGameEventManager2.Init.Hook(OnIGameEventManager2_Init, HookMode.Pre);
        CSource1LegacyGameEventGameSystem.ListenBitsReceived.Hook(OnSource1LegacyGameEventListenBitsReceived, HookMode.Pre);
    }

    public void Unload()
    {
        CSource1LegacyGameEventGameSystem.ListenBitsReceived.Unhook(OnSource1LegacyGameEventListenBitsReceived, HookMode.Pre);
        IGameEventManager2.Init.Unhook(OnIGameEventManager2_Init, HookMode.Pre);
    }

    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker) { }
    public void OnWeaponFire(EventWeaponFire @event) { }
    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle) { }

    private HookResult OnIGameEventManager2_Init(DynamicHook hook)
    {
        _gameEventManager = hook.GetParam<IGameEventManager2>(0);
        return HookResult.Continue;
    }

    private HookResult OnSource1LegacyGameEventListenBitsReceived(DynamicHook hook)
    {
        if (_gameEventManager == null)
            return HookResult.Continue;

        CSource1LegacyGameEventGameSystem pLegacyEventSystem = hook.GetParam<CSource1LegacyGameEventGameSystem>(0);
        CLCMsg_ListenEvents pMsg = hook.GetParam<CLCMsg_ListenEvents>(1);
        CPlayerSlot slot = pMsg.GetPlayerSlot();
        CServerSideClient_GameEventLegacyProxy? pClientProxyListener = pLegacyEventSystem.GetLegacyGameEventListener(slot);

        if (pClientProxyListener == null)
            return HookResult.Continue;

        CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);

        if (player == null || player.IsBot)
            return HookResult.Continue;

        if (Instance.ResultType == ResultType.PrintAll || Instance.ResultType == ResultType.PrintAdmin)
        {
            int tick = Server.TickCount;
            if (Instance.GetPlayerData(player)?.AntiDLL is not { } data || data.LastTickCount > tick)
                return HookResult.Continue;

            data.LastTickCount = tick + 5.0f;
        }

        bool hasBlacklisted = Instance.Config.Modules.AntiDLL.Blacklist.Any(eventName => _gameEventManager.FindListener(pClientProxyListener, eventName));

        if (hasBlacklisted)
        {
            Instance.OnPlayerDetected(player, CheatType.Event);
        }

        return HookResult.Continue;
    }
}