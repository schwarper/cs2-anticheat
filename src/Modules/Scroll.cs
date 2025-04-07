using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static AntiCheat.AntiCheat;

namespace AntiCheat;

public class BunnyHopDetector : ICheatDetector
{
    private readonly int _sampleSize;

    public BunnyHopDetector()
    {
        Random random = new();
        _sampleSize = random.Next(
            Instance.Config.Modules.BunnyHop.SampleSizeMin,
            Instance.Config.Modules.BunnyHop.SampleSizeMax + 1);
    }

    public void Load() { }
    public void Unload() { }
    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker) { }
    public void OnWeaponFire(EventWeaponFire @event) { }

    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle)
    {
        if (Instance.GetPlayerData(player)?.BunnyHop is not { } data ||
            player.PlayerPawn.Value is not { } playerPawn)
            return;

        bool onGround = playerPawn.GroundEntity.IsValid || playerPawn.WaterLevel >= 2;

        if (onGround)
            data.GroundTicks++;

        PlayerButtons buttons = player.Buttons;
        Vector velocity = playerPawn.AbsVelocity;
        float speed = (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);

        CollectJumpStats(player, data, onGround, buttons, velocity.Z);

        if (speed > 225.0f)
            CollectJumpStats(player, data, onGround, buttons, velocity.Z);
        else
            ResetTempStats(data);

        data.PreviousGround = onGround;
        data.PreviousButtons = buttons;
    }

    private void CollectJumpStats(CCSPlayerController player, BunnyHopData data, bool onGround, PlayerButtons buttons, float velocityZ)
    {
        State groundState = State.Nothing;
        State buttonState = State.Nothing;

        if (onGround && !data.PreviousGround)
            groundState = State.Landing;
        else if (!onGround && data.PreviousGround)
            groundState = State.Jumping;

        if (buttons.HasFlag(PlayerButtons.Jump))
        {
            if (!data.PreviousButtons.HasFlag(PlayerButtons.Jump))
            {
                buttonState = State.Pressing;
            }
        }
        else if (data.PreviousButtons.HasFlag(PlayerButtons.Jump))
        {
            buttonState = State.Releasing;
        }

        int tickCount = Server.TickCount;

        if (buttonState == State.Pressing)
        {
            data.TempStats[(int)Stats.Scrolls]++;
            data.TempStats[(int)Stats.AverageTicks] += (tickCount - data.ReleaseTick);

            if (onGround)
            {
                if (buttons.HasFlag(PlayerButtons.Jump))
                {
                    data.TempStats[(int)Stats.PerfectJump] = !data.PreviousGround ? 1 : 0;
                }
            }
            else
            {
                float distance = GetGroundDistance(player);

                if (distance < 33.0f)
                {
                    if (velocityZ > 0.0f && data.CurrentJump > 1)
                    {
                        JumpStats lastJump = data.JumpStats[^1];
                        lastJump.AfterGround++;
                    }
                    else if (velocityZ < 0.0f)
                    {
                        data.TempStats[(int)Stats.BeforeGround]++;
                    }
                }
            }
        }
        else if (buttonState == State.Releasing)
        {
            data.ReleaseTick = tickCount;
        }

        if (!onGround && data.AirTicks++ > 135)
        {
            ResetTempStats(data);
            return;
        }

        if (groundState == State.Landing)
        {
            int scrolls = data.TempStats[(int)Stats.Scrolls];

            if (scrolls == 0)
            {
                ResetTempStats(data);
                return;
            }

            if (data.GroundTicks < 8)
            {
                data.JumpStats.Add(new JumpStats
                {
                    Scrolls = scrolls,
                    BeforeGround = data.TempStats[(int)Stats.BeforeGround],
                    AfterGround = 0,
                    AverageTicks = data.TempStats[(int)Stats.AverageTicks] / scrolls,
                    PerfectJump = data.TempStats[(int)Stats.PerfectJump] > 0
                });

                data.CurrentJump++;
            }

            data.GroundTicks = 0;
            ResetTempStats(data);
        }
        else if (groundState == State.Jumping && data.CurrentJump >= _sampleSize)
        {
            AnalyzeStats(player, data);
        }
    }

    private static void ResetTempStats(BunnyHopData data)
    {
        Array.Clear(data.TempStats, 0, data.TempStats.Length);
        data.ReleaseTick = Server.TickCount;
        data.AirTicks = 0;
    }

    private static float GetGroundDistance(CCSPlayerController player)
    {
        if (player.PlayerPawn.Value?.GroundEntity.IsValid is true)
            return 0.0f;

        QAngle downAngle = new(90, player.PlayerPawn!.Value!.AbsRotation!.Y, 0);

        Vector? end = new Trace().TraceShape(
            origin: new Vector(player.PlayerPawn!.Value!.AbsOrigin!.X, player.PlayerPawn!.Value!.AbsOrigin.Y, player.PlayerPawn!.Value!.AbsOrigin.Z - 3),
            viewangles: downAngle);

        if (end != null)
        {
            Vector position = player.PlayerPawn.Value.AbsOrigin;
            return (float)Math.Sqrt(
                Math.Pow(position.X - end.X, 2) +
                Math.Pow(position.Y - end.Y, 2) +
                Math.Pow(position.Z - end.Z, 2));
        }

        return 0.0f;
    }

    private void AnalyzeStats(CCSPlayerController player, BunnyHopData data)
    {
        int perfs = GetPerfectJumps(data);

        int iVeryHighNumber = 0;
        int iSameAsNext = 0;
        int iCloseToNext = 0;
        int iBadIntervals = 0;
        int iLowBefores = 0;
        int iLowAfters = 0;
        int iSameBeforeAfter = 0;

        for (int i = data.JumpStats.Count - _sampleSize; i < data.JumpStats.Count - 1; i++)
        {
            JumpStats current = data.JumpStats[i];
            JumpStats next = data.JumpStats[i + 1];

            if (current.Scrolls == next.Scrolls)
                iSameAsNext++;

            if (Math.Abs(current.Scrolls - next.Scrolls) <= 2)
                iCloseToNext++;

            if (current.Scrolls >= 17)
                iVeryHighNumber++;

            if (current.AverageTicks <= 2)
                iBadIntervals++;

            if (current.BeforeGround <= 1)
                iLowBefores++;

            if (current.AfterGround <= 1)
                iLowAfters++;

            if (current.BeforeGround == current.AfterGround)
                iSameBeforeAfter++;
        }

        float fIntervals = iBadIntervals / (float)_sampleSize;
        bool triggered = true;

        if (perfs >= 91)
        {
            TriggerDetection(player);
        }
        else if (perfs >= 87 && (iSameAsNext >= 13 || iCloseToNext >= 18))
        {
            TriggerDetection(player);
        }
        else if (perfs >= 85 && iSameAsNext >= 13)
        {
            TriggerDetection(player);
        }
        else if (perfs >= 80 && iSameAsNext >= 15)
        {
            TriggerDetection(player);
        }
        else if (perfs >= 75 && iVeryHighNumber >= 4 && iSameAsNext >= 3 && iCloseToNext >= 10)
        {
            TriggerDetection(player);
        }
        else if (perfs >= 85 && iCloseToNext >= 16)
        {
            TriggerDetection(player);
        }
        else if (perfs >= 40 && iLowBefores >= 45)
        {
            TriggerDetection(player);
        }
        else if (perfs >= 55 && iSameBeforeAfter >= 25)
        {
            TriggerDetection(player);
        }
        else if (perfs >= 40 && iLowAfters >= 45)
        {
            TriggerDetection(player);
        }
        else if (iVeryHighNumber >= 15 && (iCloseToNext >= 13 || perfs >= 80))
        {
            TriggerDetection(player);
        }
        else if (fIntervals > 0.75f)
        {
            TriggerDetection(player);
        }
        else
        {
            triggered = false;
        }

        if (triggered)
        {
            ResetTempStats(data);
            data.CurrentJump = 0;
            data.JumpStats.Clear();
        }
    }

    private int GetPerfectJumps(BunnyHopData data)
    {
        int perfs = 0;
        int size = data.JumpStats.Count;
        int end = (size >= _sampleSize) ? (size - _sampleSize) : 0;
        int totalJumps = size - end;

        for (int i = size - 1; i >= end; i--)
        {
            if (data.JumpStats[i].PerfectJump)
                perfs++;
        }

        return totalJumps == 0 ? 0 : (int)Math.Round(perfs / (float)totalJumps * 100);
    }

    private static void TriggerDetection(CCSPlayerController player)
    {
        Instance.OnPlayerDetected(player, CheatType.Scroll);
    }
}