using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace AntiCheat;

public class IGameEventManager2(nint ptr) : NativeObject(ptr)
{
    public static readonly MemoryFunctionVoid<IGameEventManager2> Init = new(GameData.GetSignature("CGameEventManager_Init"));

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
