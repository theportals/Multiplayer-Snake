using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Multiplayer_Snake.Components;

public class Position : Component
{
    public List<Vector2> segments = new();
    public float x => segments[0].X;
    public float y => segments[0].Y;

    public Position(float x, float y)
    {
        segments.Add(new Vector2(x, y));
    }
}