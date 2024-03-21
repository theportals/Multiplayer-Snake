using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Multiplayer_Snake.Entities;
using Multiplayer_Snake.Input;
using Multiplayer_Snake.Util;

namespace Multiplayer_Snake;

public class GameModel
{
    private const int ARENA_SIZE = 750;
    private const int OBSTACLE_COUNT = 15;
    private readonly int WINDOW_WIDTH;
    private readonly int WINDOW_HEIGHT;

    private List<Entity> mToRemove = new();
    private List<Entity> mToAdd = new();

    private Systems.Renderer mSysRenderer;
    private Systems.Collision mSysCollision;
    private Systems.Movement mSysMovement;

    private KeyboardInput mKeyboardInput;
    private MouseInput mMouseInput;
    private bool mListenKeys;

    public GameModel(int width, int height, KeyboardInput keyboardInput, MouseInput mouseInput, bool listenKeys)
    {
        WINDOW_WIDTH = width;
        WINDOW_HEIGHT = height;
        mKeyboardInput = keyboardInput;
        mMouseInput = mouseInput;
        mListenKeys = listenKeys;
    }

    public void Initialize(ContentManager content, SpriteBatch spriteBatch)
    {
        var square = content.Load<Texture2D>("Images/square");

        mSysRenderer = new Systems.Renderer(spriteBatch, square, WINDOW_WIDTH, WINDOW_HEIGHT, ARENA_SIZE);
        mSysCollision = new Systems.Collision(e =>
        {
            mToRemove.Add(e);
            mToAdd.Add(createFood(square));
        },
        e =>
        {
            Console.WriteLine("We crashin!!!");
        });

        mSysMovement = new Systems.Movement();

        initializeBorder(square);
        initializeObstacles(square);
        initializeSnake(square);
        addEntity(createFood(square));
    }

    public void update(GameTime gameTime)
    {
        mKeyboardInput.update(gameTime);
        mMouseInput.update(gameTime);
        mSysCollision.Update(gameTime);

        foreach (var entity in mToRemove)
        {
            removeEntity(entity);
        }
        mToRemove.Clear();

        foreach (var entity in mToAdd)
        {
            addEntity(entity);
        }
        mToAdd.Clear();
    }

    public void Draw(GameTime gameTime)
    {
        mSysRenderer.Update(gameTime);
    }

    private void addEntity(Entity entity)
    {
        mSysMovement.Add(entity);
        mSysCollision.Add(entity);
        mSysRenderer.Add(entity);
    }

    private void removeEntity(Entity entity)
    {
        mSysMovement.Remove(entity.Id);
        mSysCollision.Remove(entity.Id);
        mSysRenderer.Remove(entity.Id);
    }

    private void initializeBorder(Texture2D square)
    {
        for (int position = 0; position < ARENA_SIZE; position += 5)
        {
            var left = BorderBlock.create(square, 0, position);
            addEntity(left);
            
            var right = BorderBlock.create(square, ARENA_SIZE, position);
            addEntity(right);
            
            var top = BorderBlock.create(square, position, 0);
            addEntity(top);
            
            var bottom = BorderBlock.create(square, position, ARENA_SIZE);
            addEntity(bottom);

            var bottomRight = BorderBlock.create(square, ARENA_SIZE, ARENA_SIZE);
            addEntity(bottomRight);
        }
    }

    private void initializeObstacles(Texture2D square)
    {
        var rng = new ExtendedRandom();
        var remaining = OBSTACLE_COUNT;

        while (remaining > 0)
        {
            int x = (int)rng.nextRange(1, ARENA_SIZE - 1);
            int y = (int)rng.nextRange(1, ARENA_SIZE - 1);
            var proposed = Obstacle.create(square, x, y);
            if (!mSysCollision.anyCollision(proposed))
            {
                addEntity(proposed);
                remaining -= 1;
            }
        }
    }

    private void initializeSnake(Texture2D square)
    {
        var rng = new ExtendedRandom();
        bool done = false;

        while (!done)
        {
            int x = (int)rng.nextRange(1, ARENA_SIZE - 1);
            int y = (int)rng.nextRange(1, ARENA_SIZE - 1);
            var proposed = SnakeSegment.create(square, mKeyboardInput, mMouseInput, mListenKeys, x, y);
            if (!mSysCollision.anyCollision(proposed))
            {
                addEntity(proposed);
                done = true;
            }
        }
    }

    private Entity createFood(Texture2D square)
    {
        var rng = new ExtendedRandom();
        var done = false;

        while (!done)
        {
            int x = (int)rng.nextRange(1, ARENA_SIZE - 1);
            int y = (int)rng.nextRange(1, ARENA_SIZE - 1);
            var proposed = Food.create(square, x, y);
            if (!mSysCollision.anyCollision(proposed))
            {
                return proposed;
            }
        }

        return null;
    }
}