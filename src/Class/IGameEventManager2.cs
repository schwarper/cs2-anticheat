using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace AntiCheat;

public class IGameEventManager2(nint ptr) : NativeObject(ptr)
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

    private static class VTable
    {
        public static readonly MemoryFunctionWithReturn<IGameEventManager2, IGameEventListener2, string, bool> FindListener =
            new(GameData.GetSignature("IGameEventManager2::FindListener"));
    }

    public bool FindListener(IGameEventListener2 listener, string eventName)
    {
        return VTable.FindListener.Invoke(this, listener, eventName);
    }
}
