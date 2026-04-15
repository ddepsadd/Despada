using System.Numerics;
using ImGuiNET;

namespace Despada.ImGui.UserInterface;

public static partial class Widgets
{
    private struct Spark
    {
        public Vector2 Pos, Vel;
        public float Life, MaxLife, Size;
    }

    private static readonly List<Spark> _sparks = new();
    private static readonly Random _rng = new();

    private static void UpdateSparks(ImDrawListPtr dl, float dt)
    {
        for (int i = _sparks.Count - 1; i >= 0; i--)
        {
            var s = _sparks[i];
            s.Life -= dt;
            if (s.Life <= 0f)
            {
                _sparks.RemoveAt(i);
                continue;
            }

            s.Pos += s.Vel * dt;
            s.Vel.Y += 150f * dt;
            _sparks[i] = s;

            var alpha = s.Life / s.MaxLife;
            var t = 1f - alpha;
            var color = new Vector4(
                Theme.Violet.X + (Theme.Magenta.X - Theme.Violet.X) * t,
                Theme.Violet.Y + (Theme.Magenta.Y - Theme.Violet.Y) * t,
                Theme.Violet.Z + (Theme.Magenta.Z - Theme.Violet.Z) * t,
                alpha);

            dl.AddCircleFilled(s.Pos, s.Size * (0.5f + 0.5f * alpha), Theme.ToU32(color));
        }
    }

    private static void SpawnSparks(Vector2 origin, float dragVelX, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var velX = -dragVelX * (0.05f + _rng.NextSingle() * 0.15f)
                     + (_rng.NextSingle() - 0.5f) * 50f;
            var velY = (_rng.NextSingle() - 0.6f) * 80f;

            var speed = MathF.Sqrt(velX * velX + velY * velY);
            if (speed > 200f) { velX *= 200f / speed; velY *= 200f / speed; }
            if (speed < 30f)  { velX *= 30f / speed;  velY *= 30f / speed; }

            var life = 0.6f + _rng.NextSingle() * 0.5f;

            _sparks.Add(new Spark
            {
                Pos     = origin + new Vector2((_rng.NextSingle() - 0.5f) * 6f, (_rng.NextSingle() - 0.5f) * 6f),
                Vel     = new Vector2(velX, velY),
                Life    = life,
                MaxLife = life,
                Size    = 1.5f + _rng.NextSingle() * 1.5f,
            });
        }
    }

    private static void SpawnBurstSparks(Vector2 center, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var angle = _rng.NextSingle() * MathF.PI * 2f;
            var speed = 60f + _rng.NextSingle() * 120f;
            var velX = MathF.Cos(angle) * speed;
            var velY = MathF.Sin(angle) * speed - 20f;

            var life = 0.5f + _rng.NextSingle() * 0.5f;

            _sparks.Add(new Spark
            {
                Pos     = center + new Vector2((_rng.NextSingle() - 0.5f) * 6f, (_rng.NextSingle() - 0.5f) * 6f),
                Vel     = new Vector2(velX, velY),
                Life    = life,
                MaxLife = life,
                Size    = 1.5f + _rng.NextSingle() * 2f,
            });
        }
    }
}