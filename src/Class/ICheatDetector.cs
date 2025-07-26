using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiCheat;

public interface ICheatDetector
{
    void Load();
    void Unload();
    void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker);
    void OnWeaponFire(CCSPlayerController player);
    void OnProcessUsercmds(CCSPlayerController player, QAngle angle);
}