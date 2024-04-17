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
    public int startScore { get; set; }
    public int goalScore { get; set; }
    public int startKills { get; set; }
    public int goalKills { get; set; }
    public float startCollisionSize { get; set; }
    public float goalCollisionSize { get; set; }
    public float startIntangibility { get; set; }
    public float goalIntangibility { get; set; }
    public TimeSpan updateWindow { get; set; }
    public TimeSpan updatedTime { get; set; }
    
    public Goal(List<Vector2> segments, float facing, float stamina, int score, int kills, float collisionSize, float intangibility)
    {
        startSegments = segments;
        goalSegments = segments;
        startFacing = facing;
        goalFacing = facing;
        startStamina = stamina;
        goalStamina = stamina;
        startScore = score;
        goalScore = score;
        startKills = kills;
        goalKills = kills;
        startCollisionSize = collisionSize;
        goalCollisionSize = collisionSize;
        startIntangibility = intangibility;
        goalIntangibility = intangibility;
    }
}