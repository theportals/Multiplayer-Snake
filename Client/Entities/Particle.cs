using Client.Util;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;
using Shared.Util;

namespace Client.Entities;

public class Particle
{
    public static Entity create(string texture, float x, float y, ExtendedRandom rng, int sizeMean, int sizeStdDev, float moveDir, float moveDirStdDev, float speed,
        float speedStdDev, float lifetime, float lifetimeStdDev, float baseRotation, float rotationSpeed, float rotationSpeedStdDev)
    {
        return new Entity()
                .add(new Shared.Components.Position(x, y))
                .add(new Appearance(texture, (int)rng.nextGaussian(sizeMean,sizeStdDev), false, 0, 0, 0))
                .add(new Shared.Components.Movable((float)rng.nextGaussian(moveDir, moveDirStdDev), (float)rng.nextGaussian(speed, speedStdDev), 0f))
                .add(new Lifetime((float)rng.nextGaussian(lifetime, lifetimeStdDev)))
                .add(new RotationOffset(baseRotation, (float)rng.nextGaussian(rotationSpeed, rotationSpeedStdDev)));
    }
}