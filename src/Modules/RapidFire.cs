using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static AntiCheat.AntiCheat;

namespace AntiCheat;

public class RapidFireDetector : ICheatDetector
{
    public void Load() { }
    public void Unload() { }
    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker) { }
    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle) { }

    public void OnWeaponFire(EventWeaponFire @event)
    {
        if (@event.Userid is not CCSPlayerController player ||
            Instance.GetPlayerData(player)?.RapidFire is not { } data)
            return;

        if (player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value?.GetVData<CCSWeaponBaseVData>() is not { } weaponData ||
            weaponData.Name == "weapon_revolver")
            return;

        int tick = Server.TickCount;

        if (tick - data.LastShotTick > weaponData.CycleTime.Values[0] * 64)
            return;

        data.SuspicionCount++;

        if (data.SuspicionCount >= Instance.Config.Modules.RapidFire.MaxSuspicion)
        {
            Instance.OnPlayerDetected(player, CheatType.RapidFire);
            data.SuspicionCount = 0;
        }

        data.LastShotTick = Server.TickCount;
    }
}