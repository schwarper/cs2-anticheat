using System.Runtime.CompilerServices;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

namespace AntiCheat;

public static class Hook_ProcessUsercmds
{
    public static readonly MemoryFunctionVoid<CCSPlayerController, IntPtr, int, CCSPlayer_MovementServices, float> ProcessUsercmds =
        new(GameData.GetSignature("ProcessUsercmds"));

    public class CUserCmd(IntPtr pointer)
    {
        private IntPtr Handle { get; set; } = pointer;

        public unsafe QAngle? GetViewAngles()
        {
            if (Handle == IntPtr.Zero)
                return null;

            nint baseCmd = Unsafe.Read<IntPtr>((void*)(Handle + 0x40));
            if (baseCmd == IntPtr.Zero)
                return null;

            nint msgQAngle = Unsafe.Read<IntPtr>((void*)(baseCmd + 0x40));
            if (msgQAngle == IntPtr.Zero)
                return null;

            QAngle viewAngles = new(msgQAngle + 0x18);
            return viewAngles.Handle != IntPtr.Zero ? viewAngles : null;
        }
    }
}