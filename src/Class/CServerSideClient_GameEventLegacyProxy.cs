using System.Runtime.InteropServices;

namespace AntiCheat;

public class CServerSideClient_GameEventLegacyProxy(nint ptr) : IGameEventListener2(ptr)
{
    private unsafe CPlayerSlot Slot => Marshal.PtrToStructure<CPlayerSlot>(Handle + 8);
    public CPlayerSlot GetPlayerSlot()
    {
        return Slot;
    }
}
