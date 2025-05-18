using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Numerics;
using System.Runtime.InteropServices;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace AntiCheat;

public class Trace()
{
    private static readonly nint TraceFunc = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("TraceFunc"));
    private static readonly nint GameTraceManager = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("GameTraceManager"));

    private readonly TraceShapeDelegate _traceShape = Marshal.GetDelegateForFunctionPointer<TraceShapeDelegate>(TraceFunc);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate bool TraceShapeDelegate(
        nint GameTraceManager,
        nint vecStart,
        nint vecEnd,
        nint skip,
        ulong mask,
        byte a6,
        GameTrace* pGameTrace
    );

    public unsafe Vector? TraceShape(Vector origin, QAngle viewangles)
    {
        Vector _forward = new();

        NativeAPI.AngleVectors(viewangles.Handle, _forward.Handle, 0, 0);
        Vector _endOrigin = new(origin.X + _forward.X * 8192, origin.Y + _forward.Y * 8192, origin.Z + _forward.Z * 8192);

        return TraceShape(origin, _endOrigin);
    }

    public unsafe Vector? TraceShape(Vector _origin, Vector _endOrigin)
    {
        try
        {
            nint _gameTraceManagerAddress = Address.GetAbsoluteAddress(GameTraceManager, 3, 7);

            GameTrace* _trace = stackalloc GameTrace[1];

            const ulong mask = 0x1C1003;
            bool result = _traceShape(*(nint*)_gameTraceManagerAddress, _origin.Handle, _endOrigin.Handle, 0, mask, 4, _trace);

            if (result)
                return new(_trace->EndPos.X, _trace->EndPos.Y, _trace->EndPos.Z);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
        return null;
    }

    public unsafe Vector? TraceShapeEx(Vector _origin, Vector _endOrigin)
    {
        try
        {
            nint _gameTraceManagerAddress = Address.GetAbsoluteAddress(GameTraceManager, 3, 7);

            GameTrace* _trace = stackalloc GameTrace[1];

            const ulong mask = 0x1 | 0x4000 | 0x80 | 0x2000;
            bool result = _traceShape(*(nint*)_gameTraceManagerAddress, _origin.Handle, _endOrigin.Handle, 0, mask, 4, _trace);

            if (result)
                return new(_trace->EndPos.X, _trace->EndPos.Y, _trace->EndPos.Z);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
        return null;
    }
}

internal static class Address
{
    static unsafe public nint GetAbsoluteAddress(nint addr, nint offset, int size)
    {
        if (addr == IntPtr.Zero)
            throw new Exception("Failed to find RayTrace signature.");

        int code = *(int*)(addr + offset);
        return addr + code + size;
    }

    static public nint GetCallAddress(nint a)
    {
        return GetAbsoluteAddress(a, 1, 5);
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x44)]
public unsafe struct TraceHitboxData
{
    [FieldOffset(0x38)] public int HitGroup;
    [FieldOffset(0x40)] public int HitboxId;
}

[StructLayout(LayoutKind.Explicit, Size = 0xB8)]
public unsafe struct GameTrace
{
    [FieldOffset(0)] public void* Surface;
    [FieldOffset(0x8)] public void* HitEntity;
    [FieldOffset(0x10)] public TraceHitboxData* HitboxData;
    [FieldOffset(0x50)] public uint Contents;
    [FieldOffset(0x78)] public Vector3 StartPos;
    [FieldOffset(0x84)] public Vector3 EndPos;
    [FieldOffset(0x90)] public Vector3 Normal;
    [FieldOffset(0x9C)] public Vector3 Position;
    [FieldOffset(0xAC)] public float Fraction;
    [FieldOffset(0xB6)] public bool AllSolid;
}