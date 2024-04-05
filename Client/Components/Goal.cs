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
    public float startStamina { get; set; }
    public float goalStamina { get; set; }
    public TimeSpan updateWindow { get; set; }
    public TimeSpan updatedTime { get; set; }
    
    public Goal(List<Vector2> segments, float facing, float stamina)
    {
        startSegments = segments;
        goalSegments = segments;
        startFacing = facing;
        goalFacing = facing;
        startStamina = stamina;
        goalStamina = stamina;
    }
}