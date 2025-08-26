using System.Runtime.InteropServices;
using CounterStrikeSharp.API;

namespace AntiCheat.Modules.AntiDLL.Class;

public class CLCMsg_ListenEvents(nint ptr) : NativeObject(ptr)
{
    private unsafe CPlayerSlot Slot => Marshal.PtrToStructure<CPlayerSlot>(Handle + 88);
    public CPlayerSlot GetPlayerSlot()
    {
        return Slot;
    }
}