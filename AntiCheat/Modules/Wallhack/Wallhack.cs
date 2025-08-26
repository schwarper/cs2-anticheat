using AntiCheat.Class;
using AntiCheat.Interface;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2TraceRay.Class;
using static AntiCheat.AntiCheat;
using static CounterStrikeSharp.API.Core.Listeners;
using CS2TraceRay.Struct;

namespace AntiCheat.Modules.Wallhack;

public class Wallhack : ICheatDetector
{
    public bool RequiresProcessUsercmdsHook => true;

    public void Load()
    {
        Instance.RegisterListener<CheckTransmit>(CheckTransmit);
    }
    public void Unload()
    {
        Instance.RemoveListener<CheckTransmit>(CheckTransmit);
    }
    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker) { }
    public void OnWeaponFire(CCSPlayerController player) { }
    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle)
    {
        PlayerData.Get(player).Wallhack.LastAngle = angle;
    }

    public static void CheckTransmit(CCheckTransmitInfoList infoList)
    {
        List<CCSPlayerController> players = Utilities.GetPlayers();

        foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
        {
            if (player == null || player.PlayerPawn.Value?.LifeState != (byte)LifeState_t.LIFE_ALIVE)
                continue;

            WallhackData playerData = PlayerData.Get(player).Wallhack;

            foreach (CCSPlayerController target in players)
            {
                if (target == player || target.Pawn.Value is not { } targetPawn || targetPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
                    continue;

                if (IsAbleToSee(player, target, playerData))
                    continue;

                info.TransmitEntities.Remove(targetPawn);
            }
        }
    }

    private static bool IsAbleToSee(CCSPlayerController player, CCSPlayerController target, WallhackData playerData)
    {
        WallhackData targetData = PlayerData.Get(target).Wallhack;

        if (playerData.LastAngle is not { } eyeAnglePlayer || targetData.LastAngle is not { } eyeAngleTarget)
            return true;

        Vector eyePosPlayer = player.GetEyePosition()!;
        Vector eyePosTarget = target.GetEyePosition()!;
        Vector originTarget = target.PlayerPawn.Value!.AbsOrigin!;

        if (IsFOV(eyePosPlayer, eyeAnglePlayer, originTarget))
        {
            if (IsPointVisible(eyePosPlayer, originTarget))
            {
                return true;
            }

            if (IsFwdVecVisible(eyePosPlayer, eyeAngleTarget, eyePosTarget))
            {
                return true;
            }

            WallhackData data = PlayerData.Get(target).Wallhack;

            data.Mins = target.PlayerPawn.Value.Collision.Mins;
            data.Maxs = target.PlayerPawn.Value.Collision.Maxs;

            if (data.Mins != null && data.Maxs != null)
            {
                data.Mins[0] -= 5;
                data.Mins[1] -= 30;
                data.Maxs[0] += 5;
                data.Maxs[1] += 5;

                AddVectors(originTarget, data.Mins, out Vector? vBoxPrimeMins);
                AddVectors(originTarget, data.Maxs, out Vector? vBoxPrimeMaxs);

                if (IsBoxVisible(vBoxPrimeMins, vBoxPrimeMaxs, eyePosPlayer))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsFOV(Vector start, QAngle angles, Vector end)
    {
        GetAngleVectors(angles, out Vector? normal);
        SubtractVectors(end, start, out Vector? plane);
        NormalizeVector(plane, out plane);

        return GetVectorDistance(start, end) < 75.0 || GetVectorDotProduct(plane, normal) > 0.0;
    }

    private static bool IsFwdVecVisible(Vector start, QAngle angles, Vector end)
    {
        GetAngleVectors(angles, out Vector? fwd);
        ScaleVector(fwd, 60.0f, out fwd);
        AddVectors(end, fwd, out fwd);

        return IsPointVisible(start, fwd);
    }

    private static bool IsBoxVisible(Vector bottomCornerVec, Vector upperCornerVec, Vector startVec)
    {
        float[] bottomCorner = [bottomCornerVec.X, bottomCornerVec.Y, bottomCornerVec.Z];
        float[] upperCorner = [upperCornerVec.X, upperCornerVec.Y, upperCornerVec.Z];
        float[] start = [startVec.X, startVec.Y, startVec.Z];
        float[,] corners = new float[8, 3];

        for (int i = 0; i < 3; i++)
        {
            corners[0, i] = bottomCorner[i];
            corners[1, i] = bottomCorner[i];
            corners[2, i] = bottomCorner[i];
            corners[3, i] = bottomCorner[i];
            corners[4, i] = upperCorner[i];
            corners[5, i] = upperCorner[i];
            corners[6, i] = upperCorner[i];
            corners[7, i] = upperCorner[i];
        }

        corners[1, 0] = upperCorner[0];
        corners[2, 0] = upperCorner[0];
        corners[2, 1] = upperCorner[1];
        corners[3, 1] = upperCorner[1];
        corners[4, 0] = bottomCorner[0];
        corners[4, 1] = bottomCorner[1];
        corners[5, 1] = bottomCorner[1];
        corners[7, 0] = bottomCorner[0];

        for (int i = 0; i < 8; i++)
        {
            Vector cornerVec = new(corners[i, 0], corners[i, 1], corners[i, 2]);
            Vector startVecCopy = new(start[0], start[1], start[2]);

            if (IsPointVisible(cornerVec, startVecCopy))
                return true;
        }

        return false;
    }

    private static bool IsPointVisible(Vector start, Vector end)
    {
        const ulong mask = 0x1 | 0x4000 | 0x80 | 0x2000;
        TraceRay.TraceShapeWithResult(start, end, mask, 4, 0, out bool result);
        return !result;
    }

    private static void GetAngleVectors(QAngle angles, out Vector forward)
    {
        float pitch = angles.X;
        float yaw = angles.Y;

        pitch = pitch * (float)Math.PI / 180.0f;
        yaw = yaw * (float)Math.PI / 180.0f;

        forward = new Vector
        {
            X = (float)(Math.Cos(pitch) * Math.Cos(yaw)),
            Y = (float)(Math.Cos(pitch) * Math.Sin(yaw)),
            Z = (float)-Math.Sin(pitch)
        };
    }

    private static void SubtractVectors(Vector v1, Vector v2, out Vector result)
    {
        result = new Vector
        {
            X = v1.X - v2.X,
            Y = v1.Y - v2.Y,
            Z = v1.Z - v2.Z
        };
    }

    private static void NormalizeVector(Vector v, out Vector result)
    {
        float length = (float)Math.Sqrt((v.X * v.X) + (v.Y * v.Y) + (v.Z * v.Z));
        result = length != 0
            ? new Vector
            {
                X = v.X / length,
                Y = v.Y / length,
                Z = v.Z / length
            }
            : new Vector(0, 0, 0);
    }

    private static float GetVectorDistance(Vector v1, Vector v2)
    {
        return (float)Math.Sqrt(Math.Pow(v2.X - v1.X, 2) + Math.Pow(v2.Y - v1.Y, 2) + Math.Pow(v2.Z - v1.Z, 2));
    }

    private static float GetVectorDotProduct(Vector v1, Vector v2)
    {
        return (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z);
    }

    private static void ScaleVector(Vector v, float scale, out Vector result)
    {
        result = new Vector
        {
            X = v.X * scale,
            Y = v.Y * scale,
            Z = v.Z * scale
        };
    }

    private static void AddVectors(Vector v1, Vector v2, out Vector result)
    {
        result = new Vector
        {
            X = v1.X + v2.X,
            Y = v1.Y + v2.Y,
            Z = v1.Z + v2.Z
        };
    }
}