using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace AntiCheat.Modules.AntiDLL.Class;

public unsafe class IGameEventManager2 : NativeObject
{
    public static unsafe nint Init()
    {
        nint addr = NativeAPI.FindSignature(Addresses.ServerPath, GameData.GetSignature("rel_GameEventManager"));

        if (addr == nint.Zero)
        {
            return -1;
        }

        const int offset = 3;

        nint rel32 = *(int*)(addr + offset);

        addr += sizeof(int) + offset;
        addr += rel32;

        return *(nint*)addr;
    }

    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool FindListenerDelegate(nint gameEventManager, nint listener, [MarshalAs(UnmanagedType.LPStr)] string eventName);
    private readonly FindListenerDelegate FindListenerFunc;

    public IGameEventManager2(nint ptr) : base(ptr)
    {
        FindListenerFunc = Marshal.GetDelegateForFunctionPointer<FindListenerDelegate>((*(nint**)Handle)[GameData.GetOffset("IGameEventManager2::FindListener")]);
    }

    public bool FindListener(IGameEventListener2 listener, string eventName)
    {
        return FindListenerFunc(Handle, listener.Handle, eventName);
    }
}