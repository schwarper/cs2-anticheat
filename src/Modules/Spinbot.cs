using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static AntiCheat.AntiCheat;

namespace AntiCheat;

public class SpinbotDetector : ICheatDetector
{
    public void Load() { }
    public void Unload() { }
    public void OnWeaponFire(CCSPlayerController player) { }
    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker)
    {
        if (Instance.GetPlayerData(attacker)?.Spinbot is not { } data)
            return;

        data.RecentlyKilled = true;
    }

    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle)
    {
        if (Instance.GetPlayerData(player)?.Spinbot is not { } data)
            return;

        double currentTick = Server.TickedTime;

        if (data.RecentlyKilled)
        {
            float angleDifference = CalculateAngleDifference(data.LastAngle, angle);

            if (angleDifference >= Instance.Config.Modules.Spinbot.MinimumAngleChange)
            {
                double elapsedTicks = currentTick - data.LastTickCount;
                double elapsedSeconds = elapsedTicks * 0.001f;

                if (elapsedSeconds > 0)
                {
                    double angularSpeed = angleDifference / elapsedSeconds;

                    if (angularSpeed > Instance.Config.Modules.Spinbot.AngularSpeedThreshold)
                    {
                        data.SuspicionCount++;

                        if (data.SuspicionCount >= Instance.Config.Modules.Spinbot.MaxSuspicion)
                        {
                            Instance.OnPlayerDetected(player, CheatType.Spinbot);
                            data.SuspicionCount = 0;
                        }
                    }
                }
            }

            data.RecentlyKilled = false;
        }

        data.LastAngle = new(angle.X, angle.Y, angle.Z);
        data.LastTickCount = currentTick;
    }

    private static float CalculateAngleDifference(QAngle a, QAngle b)
    {
        float deltaX = NormalizeAngle(a.X - b.X);
        float deltaY = NormalizeAngle(a.Y - b.Y);
        return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180.0f) angle -= 360.0f;
        while (angle < -180.0f) angle += 360.0f;
        return Math.Abs(angle);
    }
}