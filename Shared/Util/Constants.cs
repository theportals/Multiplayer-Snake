namespace Shared.Util;

public class Constants
{
    public const int ARENA_SIZE = 1000;
    public const int OBSTACLE_COUNT = 200;
    public const int FOOD_COUNT = 25;
    public const float squiggleFactor = 10;
    public const float antiSquiggleFactor = 10;
    public const float segmentDistance = 10f / (squiggleFactor + antiSquiggleFactor);
}