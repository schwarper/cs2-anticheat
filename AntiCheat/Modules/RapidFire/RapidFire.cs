using AntiCheat.Class;
using AntiCheat.Enum;
using AntiCheat.Interface;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static AntiCheat.AntiCheat;

namespace AntiCheat.Modules.RapidFire;

public class RapidFire : ICheatDetector
{
    public bool RequiresProcessUsercmdsHook => false;

    public void Load() { }
    public void Unload() { }
    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker) { }
    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle) { }

    public void OnWeaponFire(CCSPlayerController player)
    {
        RapidFireData data = PlayerData.Get(player).RapidFire;

        if (player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value?.GetVData<CCSWeaponBaseVData>() is not { } weaponData ||
            weaponData.Name is "weapon_revolver" or "weapon_healthshot")
            return;

        int tick = Server.TickCount;

        int shotTickDiff = tick - data.LastShotTick;
        double possibleAttackDiff = (weaponData.CycleTime.Values[1] * 64) - 1.25;

        if (shotTickDiff < possibleAttackDiff)
        {
            data.SuspicionCount++;

            if (data.SuspicionCount >= Instance.Config.Modules.RapidFire.MaxSuspicion)
            {
                Instance.OnPlayerDetected(player, CheatType.RapidFire);
                data.SuspicionCount = 0;
            }
        }

        data.LastShotTick = tick;
    }
}