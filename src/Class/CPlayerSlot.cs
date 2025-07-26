using System.Runtime.InteropServices;

namespace AntiCheat;

[StructLayout(LayoutKind.Explicit)]
public readonly struct CPlayerSlot
{
    [FieldOffset(0x0)]
    private readonly int Data;

    public readonly int Get()
    {
        return Data;
    }

    public static implicit operator int(CPlayerSlot slot)
    {
        return slot.Data;
    }
}