using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static AntiCheat.AntiCheat;

namespace AntiCheat;

public class AntiDuckDetector : ICheatDetector
{
    public void Load() { }
    public void Unload() { }
    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker) { }
    public void OnWeaponFire(EventWeaponFire @event) { }
    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle)
    {
        if (Instance.GetPlayerData(player)?.AntiDuck is not { } data)
            return;

        int tick = Server.TickCount;
        PlayerButtons buttons = player.Buttons;

        if (buttons.HasFlag(PlayerButtons.Bullrush))
        {
            if (data.LastTickCount <= 0.0)
                data.LastTickCount = tick;

            if (data.LastTickCount + 1 <= tick)
            {
                data.LastTickCount = tick;
                data.SuspicionCount++;

                if (data.SuspicionCount >= Instance.Config.Modules.AntiDuck.MaxSuspicion)
                {
                    Instance.OnPlayerDetected(player, CheatType.AntiDuck);
                }
            }

            if (buttons.HasFlag(PlayerButtons.Duck) && player.PlayerPawn.Value?.MovementServices is { } movementService)
            {
                new CCSPlayer_MovementServices(movementService.Handle).DuckSpeed = 0.0f;
            }
        }
        else if (data.SuspicionCount > 0 && data.LastTickCount + 7.0 <= tick)
        {
            data.SuspicionCount = 0;
        }
    }
}