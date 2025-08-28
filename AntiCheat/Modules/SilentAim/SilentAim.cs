using AntiCheat.Class;
using AntiCheat.Enum;
using AntiCheat.Interface;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static AntiCheat.AntiCheat;

namespace AntiCheat.Modules.SilentAim;

public class SilentAim : ICheatDetector
{
    public bool RequiresProcessUsercmdsHook => true;

    public void Load() { }
    public void Unload() { }
    public void OnWeaponFire(CCSPlayerController player) { }

    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker)
    {
        SilentAimData data = PlayerData.Get(attacker).SilentAim;
        data.Victim = victim;
        data.RecentlyKilled = true;
    }

    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle)
    {
        SilentAimData data = PlayerData.Get(player).SilentAim;

      
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

        // -------------------------
        // Recoil & Spread Detection
        // -------------------------
        TrackRecoilSpread(player, angle, data);
    }

    private static void TrackRecoilSpread(CCSPlayerController player, QAngle eyeAngle, SilentAimData data)
    {
        Vector currentForward = AngleToForward(eyeAngle);

        if (data.RecentShots == null)
            data.RecentShots = new List<Vector>();

        data.RecentShots.Add(currentForward);

        if (data.RecentShots.Count > 20)
            data.RecentShots.RemoveAt(0);

        if (data.RecentShots.Count >= 5)
        {
            float totalDeviation = 0f;
            for (int i = 1; i < data.RecentShots.Count; i++)
            {
                totalDeviation += AngleBetweenVectors(data.RecentShots[i - 1], data.RecentShots[i]);
            }

            float averageDeviation = totalDeviation / (data.RecentShots.Count - 1);

          
            if (averageDeviation < Instance.Config.Modules.SilentAim.RecoilSpreadThreshold)
            {
                Instance.OnPlayerDetected(player, CheatType.SilentAim);
                data.RecentShots.Clear();
            }
        }
    }

    private static float AngleBetweenVectors(Vector a, Vector b)
    {
        float dot = Dot(a, b);
        return MathF.Acos(Math.Clamp(dot, -1f, 1f)) * (180f / MathF.PI);
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
