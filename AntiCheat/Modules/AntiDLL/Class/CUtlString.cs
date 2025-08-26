using System.Runtime.InteropServices;

namespace AntiCheat.Modules.AntiDLL.Class;

[StructLayout(LayoutKind.Explicit)]
public struct CUtlString
{
    [FieldOffset(0x0)]
    public string String;

    public readonly string Get()
    {
        return String;
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is CUtlString utlString)
        {
            return this == utlString;
        }
        else if (obj is string str)
        {
            return this == str;
        }

        return false;
    }

    public override readonly int GetHashCode()
    {
        return String?.GetHashCode() ?? 0;
    }

    public override readonly string ToString()
    {
        return String;
    }

    public static bool operator ==(CUtlString a, CUtlString b)
    {
        return a.String == b.String;
    }

    public static bool operator !=(CUtlString a, CUtlString b)
    {
        return a.String != b.String;
    }

    public static bool operator ==(CUtlString a, string b)
    {
        return a.String == b;
    }

    public static bool operator !=(CUtlString a, string b)
    {
        return a.String != b;
    }

    public static implicit operator string(CUtlString str)
    {
        return str.String;
    }
}