using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static AntiCheat.AntiCheat;

namespace AntiCheat;

public class TeleportDetector : ICheatDetector
{
    public void Load() { }
    public void Unload() { }
    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker) { }
    public void OnWeaponFire(CCSPlayerController player) { }

    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle)
    {
        if (Instance.GetPlayerData(player)?.Teleport is not { } data || angle.IsValid())
            return;

        angle.Fix();

        if (Instance.ResultType == ResultType.PrintAll || Instance.ResultType == ResultType.PrintAdmin)
        {
            int tick = Server.TickCount;
            if (data.LastTickCount > tick)
                return;

            data.LastTickCount = tick + 5.0f;
        }

        data.SuspicionCount++;

        if (data.SuspicionCount > Instance.Config.Modules.Teleport.MaxSuspicion)
        {
            Instance.OnPlayerDetected(player, CheatType.Teleport);
            data.SuspicionCount = 0;
        }
    }
}