using System;
using Microsoft.Xna.Framework;

namespace Multiplayer_Snake.Input;

/// <summary>
/// Delayed Auto Shift (DAS) Timer. Typically used to rapidly scroll through menu options after a short delay.
/// </summary>
public class DASTimer
{
    private double elapsedTime;
    private double delay;
    private double period;
    private bool delayFinished;
        
    public DASTimer(double delay, double period)
    {
        this.delay = delay;
        this.period = period;
        elapsedTime = 0;
        delayFinished = false;
    }

    public void tick(TimeSpan time, Action onTick)
    {
        elapsedTime += time.TotalMilliseconds;
        if (elapsedTime >= delay && !delayFinished)
        {
            elapsedTime = 0;
            delayFinished = true;
            onTick.Invoke();
        }

        if (delayFinished && elapsedTime >= period)
        {
            elapsedTime = 0;
            onTick.Invoke();
        }
    }

    public void resetTimer(TimeSpan time)
    {
        elapsedTime = 0;
        delayFinished = false;
    }
}