using System.Numerics;
using System.Text;
using Client.Components;
using Shared.Components;
using Shared.Entities;

namespace Shared.Messages;

public class NewEntity : Message
{
    public NewEntity(Entity entity) : base(Type.NewEntity)
    {
        id = entity.id;

        if (entity.contains<Appearance>())
        {
            hasAppearance = true;
            texture = entity.get<Appearance>().texture;
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
        }

        if (entity.contains<Components.Input>())
        {
            hasInput = true;
            inputs = entity.get<Components.Input>().inputs;
        }
        else
        {
            inputs = new List<Components.Input.Type>();
        }
    }

    public NewEntity() : base(Type.NewEntity)
    {
        texture = "";
        segments = new List<Vector2>();
        inputs = new List<Components.Input.Type>();
    }
    
    public uint id { get; private set; }
    public bool hasAppearance { get; private set; } = false;
    public string texture { get; private set; }

    public bool hasPosition { get; private set; } = false;
    public List<Vector2> segments { get; private set; }

    public bool hasMovement { get; private set; } = false;
    public float facing { get; private set; }
    public float moveSpeed { get; private set; }
    public float turnSpeed { get; private set; }

    public bool hasInput { get; private set; } = false;
    public List<Components.Input.Type> inputs { get; private set; }

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
        }
        
        data.AddRange(BitConverter.GetBytes(hasInput));
        if (hasInput)
        {
            data.AddRange(BitConverter.GetBytes(inputs.Count));
            foreach (var input in inputs)
            {
                data.AddRange(BitConverter.GetBytes((UInt16)input));
            }
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
            facing = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
        }

        hasMovement = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasMovement)
        {
            moveSpeed = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
            turnSpeed = BitConverter.ToSingle(data, offset);
            offset += sizeof(Single);
        }

        hasInput = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasInput)
        {
            var howMany = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            for (int i = 0; i < howMany; i++)
            {
                inputs.Add((Components.Input.Type)BitConverter.ToUInt16(data, offset));
                offset += sizeof(UInt16);
            }
        }

        return offset;
    }
}