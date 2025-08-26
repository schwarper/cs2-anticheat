using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace AntiCheat.Modules.AntiDLL.Class;

public class CSource1LegacyGameEventGameSystem(nint ptr) : NativeObject(ptr)
{
    public static readonly MemoryFunctionWithReturn<CSource1LegacyGameEventGameSystem, CLCMsg_ListenEvents, bool> ListenBitsReceived =
        new(GameData.GetSignature("CSource1LegacyGameEventGameSystem::ListenBitsReceived"));

    private static class Offsets
    {
        public static readonly int Listeners = GameData.GetOffset("CSource1LegacyGameEventGameSystem::Listeners");
    }

    public unsafe CUtlString Name => Marshal.PtrToStructure<CUtlString>(Handle + 8);

    public CServerSideClient_GameEventLegacyProxy? GetLegacyGameEventListener(CPlayerSlot slot)
    {
        return slot < 0 || slot > 63
            ? throw new IndexOutOfRangeException($"No proxy listener for slot '{slot.Get()}'")
            : new CServerSideClient_GameEventLegacyProxy(Handle + (16 * slot) + Offsets.Listeners);
    }
}