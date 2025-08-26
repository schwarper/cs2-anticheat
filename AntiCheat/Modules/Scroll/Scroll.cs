using AntiCheat.Class;
using AntiCheat.Enum;
using AntiCheat.Interface;
using AntiCheat.Modules.Scroll.Enum;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using static AntiCheat.AntiCheat;

namespace AntiCheat.Modules.Scroll;

public class Scroll : ICheatDetector
{
    private readonly int _sampleSize;

    public Scroll()
    {
        Random random = new();
        _sampleSize = random.Next(
            Instance.Config.Modules.Scroll.SampleSizeMin,
            Instance.Config.Modules.Scroll.SampleSizeMax + 1);
    }

    public bool RequiresProcessUsercmdsHook => true;

    public void Load() { }
    public void Unload() { }
    public void OnPlayerDeath(CCSPlayerController victim, CCSPlayerController attacker) { }
    public void OnWeaponFire(CCSPlayerController player) { }

    public void OnProcessUsercmds(CCSPlayerController player, QAngle angle)
    {
        if (player.PlayerPawn.Value is not { } playerPawn)
            return;

        var data = PlayerData.Get(player).BunnyHop;
        bool onGround = (playerPawn.Flags & (int)PlayerFlags.FL_ONGROUND) != 0 || playerPawn.WaterLevel >= 2;

        if (onGround)
            data.GroundTicks++;

        PlayerButtons buttons = player.Buttons;
        Vector velocity = playerPawn.AbsVelocity;
        float speed = velocity.Length2D();

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

        if (buttons.HasFlag(PlayerButtons.Jump) && !data.PreviousButtons.HasFlag(PlayerButtons.Jump))
            buttonState = State.Pressing;
        else if (!buttons.HasFlag(PlayerButtons.Jump) && data.PreviousButtons.HasFlag(PlayerButtons.Jump))
            buttonState = State.Releasing;

        int tickCount = Server.TickCount;

        if (buttonState == State.Pressing)
        {
            data.TempStats[(int)Stats.Scrolls]++;
            data.TempStats[(int)Stats.AverageTicks] += tickCount - data.ReleaseTick;

            if (onGround)
            {
                data.TempStats[(int)Stats.PerfectJump] = !data.PreviousGround ? 1 : 0;
            }
        }
        else if (buttonState == State.Releasing)
        {
            data.ReleaseTick = tickCount;
        }

        if (groundState == State.Landing)
        {
            int scrolls = data.TempStats[(int)Stats.Scrolls];
            if (scrolls > 0 && data.GroundTicks < 8)
            {
                data.JumpStats.Add(new JumpStats
                {
                    Scrolls = scrolls,
                    BeforeGround = data.TempStats[(int)Stats.BeforeGround],
                    AfterGround = 0,
                    AverageTicks = scrolls > 0 ? data.TempStats[(int)Stats.AverageTicks] / scrolls : 0,
                    PerfectJump = data.TempStats[(int)Stats.PerfectJump] > 0
                });
                data.CurrentJump++;
            }
            data.GroundTicks = 0;
            ResetTempStats(data);
        }

        if (data.CurrentJump >= _sampleSize)
        {
            AnalyzeStats(player, data);
        }
    }

    private static void ResetTempStats(BunnyHopData data)
    {
        Array.Clear(data.TempStats, 0, data.TempStats.Length);
        data.ReleaseTick = Server.TickCount;
    }

    private void AnalyzeStats(CCSPlayerController player, BunnyHopData data)
    {
        int suspicionScore = 0;

        int perfectJumps = 0;
        int sameScrollsCount = 0;
        int closeScrollsCount = 0;
        int veryHighScrolls = 0;
        int badIntervals = 0;
        var recentJumps = data.JumpStats.Skip(Math.Max(0, data.JumpStats.Count - _sampleSize)).ToList();

        for (int i = 0; i < recentJumps.Count - 1; i++)
        {
            var current = recentJumps[i];
            var next = recentJumps[i + 1];

            if (current.PerfectJump) perfectJumps++;
            if (current.Scrolls == next.Scrolls) sameScrollsCount++;
            if (Math.Abs(current.Scrolls - next.Scrolls) <= 2) closeScrollsCount++;
            if (current.Scrolls >= 17) veryHighScrolls++;
            if (current.AverageTicks <= 2) badIntervals++;
        }

        if (recentJumps.LastOrDefault()?.PerfectJump == true) perfectJumps++;

        float perfectJumpRatio = (float)perfectJumps / recentJumps.Count;

        if (perfectJumpRatio > 0.90f) suspicionScore += 60;
        else if (perfectJumpRatio > 0.80f) suspicionScore += 40;

        if ((float)sameScrollsCount / recentJumps.Count > 0.5f) suspicionScore += 30;
        if ((float)closeScrollsCount / recentJumps.Count > 0.7f) suspicionScore += 20;

        if (veryHighScrolls > 5) suspicionScore += 25;
        if ((float)badIntervals / recentJumps.Count > 0.75f) suspicionScore += 50;

        const int detectionThreshold = 100;

        if (suspicionScore >= detectionThreshold)
        {
            TriggerDetection(player);
        }

        data.JumpStats.Clear();
        data.CurrentJump = 0;
    }

    private static void TriggerDetection(CCSPlayerController player)
    {
        Instance.OnPlayerDetected(player, CheatType.Scroll);
    }
}