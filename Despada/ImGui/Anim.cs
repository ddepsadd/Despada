using System.Numerics;

namespace Despada.ImGui;

public static class Anim
{
    public static float Lerp(float current, float target, float speed, float dt)
    {
        if (MathF.Abs(current - target) < 0.001f)
            return target;
        return current + (target - current) * Clamp01(1f - MathF.Exp(-speed * dt));
    }

    public static Vector4 Lerp(Vector4 current, Vector4 target, float speed, float dt)
    {
        float t = Clamp01(1f - MathF.Exp(-speed * dt));
        return current + (target - current) * t;
    }

    private static float Clamp01(float v)
        => v < 0f ? 0f : v > 1f ? 1f : v;
}