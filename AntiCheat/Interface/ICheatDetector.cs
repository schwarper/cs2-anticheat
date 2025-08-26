using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiCheat.Interface;

public interface ICheatDetector
{
    bool RequiresProcessUsercmdsHook { get; }

    void Load();
    void Unload();
    void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker);
    void OnWeaponFire(CCSPlayerController player);
    void OnProcessUsercmds(CCSPlayerController player, QAngle angle);
}