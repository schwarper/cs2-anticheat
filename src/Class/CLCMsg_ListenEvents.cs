using CounterStrikeSharp.API;
using System.Runtime.InteropServices;

namespace AntiCheat;

public class CLCMsg_ListenEvents(nint ptr) : NativeObject(ptr)
{
    private unsafe CPlayerSlot Slot => Marshal.PtrToStructure<CPlayerSlot>(Handle + 80);
    public CPlayerSlot GetPlayerSlot()
    {
        return Slot;
    }
}