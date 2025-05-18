using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static AntiCheat.AntiCheat;

namespace AntiCheat;

public class SilentAimDetector : ICheatDetector
{
    public void Load() { }
    public void Unload() { }
    public void OnWeaponFire(CCSPlayerController player) { }

    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker)
    {
        if (Instance.GetPlayerData(attacker)?.SilentAim is not { } data)
            return;

        data.Victim = victim;
        data.RecentlyKilled = true;
    }

    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle)
    {
        if (Instance.GetPlayerData(player)?.SilentAim is not { } data)
            return;

        if (data.RecentlyKilled)
        {
            if (!IsLookingAtPlayer(player, data.Victim!, angle))
            {
                data.SuspicionCount++;

                if (data.SuspicionCount >= Instance.Config.Modules.SilentAim.MaxSuspicion)
                {
                    Instance.OnPlayerDetected(player, CheatType.SilentAim);
                    data.SuspicionCount = 0;
                }
            }
            data.RecentlyKilled = false;
        }
    }

    private static bool IsLookingAtPlayer(CCSPlayerController player, CCSPlayerController target, QAngle eyeAngle)
    {
        Vector forward = AngleToForward(eyeAngle);

        Vector playerPos = player.PlayerPawn.Value!.AbsOrigin!;
        Vector targetPos = target.PlayerPawn.Value!.AbsOrigin!;
        Vector directionToTarget = Normalize(targetPos - playerPos);

        float dot = Dot(forward, directionToTarget);
        float angleBetween = MathF.Acos(dot) * (180f / MathF.PI);

        return angleBetween <= Instance.Config.Modules.SilentAim.AngleThreshold;
    }

    private static Vector AngleToForward(QAngle angles)
    {
        float pitch = MathF.PI / 180f * angles.X;
        float yaw = MathF.PI / 180f * angles.Y;

        float x = MathF.Cos(pitch) * MathF.Cos(yaw);
        float y = MathF.Cos(pitch) * MathF.Sin(yaw);
        float z = -MathF.Sin(pitch);

        return Normalize(new Vector(x, y, z));
    }

    private static Vector Normalize(Vector vector)
    {
        float length = vector.Length();
        return length == 0f ? Vector.Zero : vector / length;
    }

    private static float Dot(Vector a, Vector b)
    {
        return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
    }
}