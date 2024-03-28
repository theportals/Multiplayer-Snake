using Client.Util;
using Microsoft.Xna.Framework.Graphics;
using Shared.Entities;

namespace Client.Entities;

public class Particle
{
    public static Entity create(Texture2D sprite, float x, float y, ExtendedRandom rng, int sizeMean, int sizeStdDev, float moveDir, float moveDirStdDev, float speed,
        float speedStdDev, float lifetime, float lifetimeStdDev, float baseRotation, float rotationSpeed, float rotationSpeedStdDev)
    {
        return new Entity()
                .add(new Shared.Components.Position(x, y))
                .add(new Components.Appearance(sprite, (int)rng.nextGaussian(sizeMean,sizeStdDev)))
                .add(new Shared.Components.Movable((float)rng.nextGaussian(moveDir, moveDirStdDev), (float)rng.nextGaussian(speed, speedStdDev), 0f))
                .add(new Components.Lifetime((float)rng.nextGaussian(lifetime, lifetimeStdDev)))
                .add(new Components.RotationOffset(baseRotation, (float)rng.nextGaussian(rotationSpeed, rotationSpeedStdDev)));
    }
}