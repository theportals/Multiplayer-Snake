using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Multiplayer_Snake.Components;

public class Position : Component
{
    public List<Point> segments = new();
    public int x => segments[0].X;
    public int y => segments[0].Y;

    public Position(int x, int y)
    {
        segments.Add(new Point(x, y));
    }
}