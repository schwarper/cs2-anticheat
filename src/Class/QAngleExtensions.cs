using CounterStrikeSharp.API.Modules.Utils;

namespace AntiCheat;

public static class QAngleExtensions
{
    public static bool IsValid(this QAngle angle)
    {
        return angle.IsFinite() && !angle.IsNaN() && angle.IsReasonable();
    }

    public static bool IsFinite(this QAngle angle)
    {
        return float.IsFinite(angle.X) && float.IsFinite(angle.Y) && float.IsFinite(angle.Z);
    }

    public static bool IsNaN(this QAngle angle)
    {
        return float.IsNaN(angle.X) || float.IsNaN(angle.Y) || float.IsNaN(angle.Z);
    }

    public static void Fix(this QAngle angle)
    {
        angle.FixInfinity();
        angle.FixNaN();

        if (angle.IsReasonable())
            angle.Normalize();

        angle.Clamp();
    }

    private static void FixInfinity(this QAngle angle)
    {
        if (!float.IsFinite(angle.X))
            angle.X = 0;
        if (!float.IsFinite(angle.Y))
            angle.Y = 0;
        if (!float.IsFinite(angle.Z))
            angle.Z = 0;
    }

    private static void FixNaN(this QAngle angle)
    {
        if (float.IsNaN(angle.X))
            angle.X = 0;
        if (float.IsNaN(angle.Y))
            angle.Y = 0;
        if (float.IsNaN(angle.Z))
            angle.Z = 0;
    }

    private static void Clamp(this QAngle angle)
    {
        angle.X = Math.Clamp(angle.X, -179.0f, 179.0f);
        angle.Y = Math.Clamp(angle.Y, -180.0f, 180.0f);
    }

    private static void Normalize(this QAngle angle)
    {
        angle.X = (angle.X + 180.0f) % 360.0f - 180.0f;
        angle.Y = (angle.Y + 180.0f) % 360.0f - 180.0f;
        angle.Z = (angle.Z + 180.0f) % 360.0f - 180.0f;
    }

    private static bool IsReasonable(this QAngle q)
    {
        const float r = 360.0f * 1000.0f;
        return
            q.X is > -r and < r &&
            q.Y is > -r and < r &&
            q.Z is > -r and < r;
    }
}