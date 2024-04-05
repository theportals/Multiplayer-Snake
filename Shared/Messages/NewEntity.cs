using System.Numerics;
using System.Text;
using System.Text.Unicode;
using Shared.Components;
using Shared.Entities;

namespace Shared.Messages;

public class NewEntity : Message
{
    public NewEntity(Entity entity, bool suggestFollow=false) : base(Type.NewEntity)
    {
        id = entity.id;
        this.suggestFollow = suggestFollow;

        if (entity.contains<Appearance>())
        {
            var appearance = entity.get<Appearance>();
            hasAppearance = true;
            texture = appearance.texture;
            displaySize = appearance.displaySize;
            animated = appearance.animated;
            frames = appearance.frames;
            frameWidth = appearance.frameWidth;
            frameHeight = appearance.frameHeight;
            staticFrame = appearance.staticFrame;
        }
        else
        {
            texture = "";
        }

        if (entity.contains<Position>())
        {
            hasPosition = true;
            segments = entity.get<Position>().segments;
        }
        else
        {
            segments = new List<Vector2>();
        }

        if (entity.contains<Movable>())
        {
            hasMovement = true;
            moveSpeed = entity.get<Movable>().moveSpeed;
            turnSpeed = entity.get<Movable>().turnSpeed;
            facing = entity.get<Movable>().facing;
            segmentsToAdd = entity.get<Movable>().segmentsToAdd;
        }

        if (entity.contains<RotationOffset>())
        {
            hasRotationOffset = true;
            rotationOffset = entity.get<RotationOffset>().offset;
            rotationSpeed = entity.get<RotationOffset>().rotationSpeed;
        }

        if (entity.contains<ColorOverride>())
        {
            hasColorOverride = true;
            var c = entity.get<ColorOverride>().color;
            cR = c.R;
            cG = c.G;
            cB = c.B;
        }

        if (entity.contains<PlayerName>())
        {
            hasPlayername = true;
            playerName = entity.get<PlayerName>().playerName;
        }

        if (entity.contains<Controllable>())
        {
            controllable = true;
        }

        if (entity.contains<Boostable>())
        {
            boostable = true;
            var boost = entity.get<Boostable>();
            maxStamina = boost.maxStamina;
            stamina = boost.stamina;
            regenRate = boost.regenRate;
            staminaDrain = boost.staminaDrain;
            speedModifier = boost.speedModifier;
            penaltySpeed = boost.penaltySpeed;
            boosting = boost.boosting;
        }

        if (entity.contains<Alive>())
        {
            alive = true;
        }

        if (entity.contains<Snakeitude>())
        {
            snakeitude = true;
        }

        if (entity.contains<Components.Food>())
        {
            food = true;
            naturalSpawn = entity.get<Components.Food>().naturalSpawn;
        }
    }

    public NewEntity() : base(Type.NewEntity)
    {
        texture = "";
        segments = new List<Vector2>();
    }
    
    public uint id { get; private set; }
    
    // Appearance
    public bool hasAppearance { get; private set; } = false;
    public string texture { get; private set; }
    public int displaySize { get; private set; }
    public bool animated { get; private set; }
    public int frames { get; private set; }
    public int frameWidth { get; private set; }
    public int frameHeight { get; private set; }
    public int? staticFrame { get; private set; }

    // Position
    public bool hasPosition { get; private set; } = false;
    public List<Vector2> segments { get; private set; }

    // Movement
    public bool hasMovement { get; private set; } = false;
    public float moveSpeed { get; private set; }
    public float turnSpeed { get; private set; }
    public float facing { get; private set; }
    public int segmentsToAdd { get; private set; }
    
    // Rotation Offset
    public bool hasRotationOffset { get; private set; } = false;
    public float rotationOffset { get; private set; }
    public float rotationSpeed { get; private set; }
    
    // Color override
    public bool hasColorOverride { get; private set; } = false;
    public int cR { get; private set; }
    public int cG { get; private set; }
    public int cB { get; private set; }
    
    // Player name
    public bool hasPlayername { get; private set; } = false;
    public string playerName { get; private set; }
    
    // Camera following
    public bool suggestFollow { get; private set; } = false;
    
    // Controllable
    public bool controllable { get; private set; } = false;
    
    // Boostable
    public bool boostable { get; private set; } = false;
    public float maxStamina { get; private set; }
    public float stamina { get; private set; }
    public float regenRate { get; private set; }
    public float staminaDrain { get; private set; }
    public float speedModifier { get; private set; }
    public float penaltySpeed { get; private set; }
    public bool boosting { get; private set; } = false;
    
    // Alive
    public bool alive { get; private set; } = false;
    
    // Snakeitude
    public bool snakeitude { get; private set; } = false;
    
    // Food
    public bool food { get; private set; } = false;
    public bool naturalSpawn { get; private set; } = true;

    public override byte[] serialize()
    {
        var data = new List<byte>();
        
        data.AddRange(base.serialize());
        data.AddRange(BitConverter.GetBytes(id));
        
        data.AddRange(BitConverter.GetBytes(hasAppearance));
        if (hasAppearance)
        {
            data.AddRange(BitConverter.GetBytes(texture.Length));
            data.AddRange(Encoding.UTF8.GetBytes(texture));
            data.AddRange(BitConverter.GetBytes(displaySize));
            data.AddRange(BitConverter.GetBytes(animated));
            data.AddRange(BitConverter.GetBytes(frames));
            data.AddRange(BitConverter.GetBytes(frameWidth));
            data.AddRange(BitConverter.GetBytes(frameHeight));
            
            data.AddRange(BitConverter.GetBytes(staticFrame.HasValue));
            if (staticFrame.HasValue) data.AddRange(BitConverter.GetBytes(staticFrame.Value));
        }
        
        data.AddRange(BitConverter.GetBytes(hasPosition));
        if (hasPosition)
        {
            data.AddRange(BitConverter.GetBytes(segments.Count));
            foreach (var segment in segments)
            {
                data.AddRange(BitConverter.GetBytes(segment.X));
                data.AddRange(BitConverter.GetBytes(segment.Y));
            }
        }
        
        data.AddRange(BitConverter.GetBytes(hasMovement));
        if (hasMovement)
        {
            data.AddRange(BitConverter.GetBytes(moveSpeed));
            data.AddRange(BitConverter.GetBytes(turnSpeed));
            data.AddRange(BitConverter.GetBytes(facing));
            data.AddRange(BitConverter.GetBytes(segmentsToAdd));
        }
        
        data.AddRange(BitConverter.GetBytes(hasRotationOffset));
        if (hasRotationOffset)
        {
            data.AddRange(BitConverter.GetBytes(rotationOffset));
            data.AddRange(BitConverter.GetBytes(rotationSpeed));
        }
        
        data.AddRange(BitConverter.GetBytes(hasColorOverride));
        if (hasColorOverride)
        {
            data.AddRange(BitConverter.GetBytes(cR));
            data.AddRange(BitConverter.GetBytes(cG));
            data.AddRange(BitConverter.GetBytes(cB));
        }
        
        data.AddRange(BitConverter.GetBytes(hasPlayername));
        if (hasPlayername)
        {
            data.AddRange(BitConverter.GetBytes(playerName.Length));
            data.AddRange(Encoding.UTF8.GetBytes(playerName));
        }
        
        data.AddRange(BitConverter.GetBytes(suggestFollow));
        
        data.AddRange(BitConverter.GetBytes(controllable));
        
        data.AddRange(BitConverter.GetBytes(boostable));
        if (boostable)
        {
            data.AddRange(BitConverter.GetBytes(maxStamina));
            data.AddRange(BitConverter.GetBytes(stamina));
            data.AddRange(BitConverter.GetBytes(regenRate));
            data.AddRange(BitConverter.GetBytes(staminaDrain));
            data.AddRange(BitConverter.GetBytes(speedModifier));
            data.AddRange(BitConverter.GetBytes(penaltySpeed));
            data.AddRange(BitConverter.GetBytes(boosting));
        }
        
        data.AddRange(BitConverter.GetBytes(alive));
        
        data.AddRange(BitConverter.GetBytes(snakeitude));
        
        data.AddRange(BitConverter.GetBytes(food));
        if (food)
        {
            data.AddRange(BitConverter.GetBytes(naturalSpawn));
        }

        return data.ToArray();
    }

    public override int parse(byte[] data)
    {
        var offset = base.parse(data);

        id = BitConverter.ToUInt32(data, offset);
        offset += sizeof(uint);

        hasAppearance = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasAppearance)
        {
            var textureSize = BitConverter.ToInt32(data, offset);
            offset += sizeof(Int32);
            texture = Encoding.UTF8.GetString(data, offset, textureSize);
            offset += textureSize;
            displaySize = BitConverter.ToInt32(data, offset);
            offset += sizeof(Int32);
            animated = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            frames = BitConverter.ToInt32(data, offset);
            offset += sizeof(Int32);
            frameWidth = BitConverter.ToInt32(data, offset);
            offset += sizeof(Int32);
            frameHeight = BitConverter.ToInt32(data, offset);
            offset += sizeof(Int32);
            
            var hasStatic = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasStatic)
            {
                staticFrame = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
            }
            else staticFrame = null;
        }

        hasPosition = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasPosition)
        {
            var howMany = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            for (int i = 0; i < howMany; i++)
            {
                var x = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                var y = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                segments.Add(new Vector2(x, y));
            }
        }
        
        hasMovement = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasMovement)
        {
            moveSpeed = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            turnSpeed = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            facing = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            segmentsToAdd = BitConverter.ToInt32(data, offset);
            offset += sizeof(Int32);
        }
        
        hasRotationOffset = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasRotationOffset)
        {
            rotationOffset = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            rotationSpeed = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
        }
        
        hasColorOverride = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasColorOverride)
        {
            cR = BitConverter.ToInt32(data, offset);
            offset += sizeof(Int32);
            cG = BitConverter.ToInt32(data, offset);
            offset += sizeof(Int32);
            cB = BitConverter.ToInt32(data, offset);
            offset += sizeof(Int32);
        }

        hasPlayername = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasPlayername)
        {
            var nameSize = BitConverter.ToInt32(data, offset);
            offset += sizeof(Int32);
            playerName = Encoding.UTF8.GetString(data, offset, nameSize);
            offset += nameSize;
        }

        suggestFollow = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        
        controllable = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);

        boostable = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (boostable)
        {
            maxStamina = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            stamina = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            regenRate = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            staminaDrain = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            speedModifier = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            penaltySpeed = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            boosting = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
        }

        alive = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);

        snakeitude = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);

        food = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (food)
        {
            naturalSpawn = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
        }

        return offset;
    }
}