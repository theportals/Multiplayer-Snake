using System;
using System.Collections.Generic;
using System.Numerics;
using Shared.Components;

namespace Client.Components;

public class Goal : Component
{
    public List<Vector2> startSegments { get; set; }
    public List<Vector2> goalSegments { get; set; }
    public float startFacing { get; set; }
    public float goalFacing { get; set; }
    public TimeSpan updateWindow { get; set; }
    public TimeSpan updatedTime { get; set; }
    
    public Goal(List<Vector2> segments, float facing)
    {
        startSegments = segments;
        goalSegments = segments;
        startFacing = facing;
        goalFacing = facing;
    }
}