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

    public static Vector2 Lerp(Vector2 current, Vector2 target, float speed, float dt)
    {
        float t = Clamp01(1f - MathF.Exp(-speed * dt));
        return current + (target - current) * t;
    }

    public static Vector4 Lerp(Vector4 current, Vector4 target, float speed, float dt)
    {
        float t = Clamp01(1f - MathF.Exp(-speed * dt));
        return current + (target - current) * t;
    }
    public static float Drive(float current, bool active, float speed, float dt)
        => Lerp(current, active ? 1f : 0f, speed, dt);

    public static float Clamp(float v, float min, float max)
        => v < min ? min : v > max ? max : v;

    private static float Clamp01(float v)
        => v < 0f ? 0f : v > 1f ? 1f : v;
}