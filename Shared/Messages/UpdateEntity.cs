using System.Numerics;
using Shared.Components;
using Shared.Entities;

namespace Shared.Messages;

public class UpdateEntity : Message
{
        public UpdateEntity(Entity entity, TimeSpan updateWindow) : base(Type.UpdateEntity)
        {
            id = entity.id;

            if (entity.contains<Position>())
            {
                hasPosition = true;
                segments = entity.get<Position>().segments;
            }

            if (entity.contains<Movable>())
            {
                hasMovement = true;
                facing = entity.get<Movable>().facing;
            }

            if (entity.contains<Boostable>())
            {
                hasBoost = true;
                stamina = entity.get<Boostable>().stamina;
            }

            if (entity.contains<PlayerInfo>())
            {
                hasPlayerInfo = true;
                score = entity.get<PlayerInfo>().score;
                kills = entity.get<PlayerInfo>().kills;
            }

            if (entity.contains<Collision>())
            {
                hasCollision = true;
                collisionSize = entity.get<Collision>().size;
                intangibility = entity.get<Collision>().intangibility;
            }

            this.updateWindow = updateWindow;
        }

        public UpdateEntity() : base(Type.UpdateEntity)
        {
            segments = new List<Vector2>();
        }

        public uint id { get; private set; }

        // Position
        public bool hasPosition { get; private set; } = false;
        public List<Vector2> segments { get; private set; }
        
        // Movement
        public bool hasMovement { get; private set; } = false;
        public float facing { get; private set; }
        
        // Boost
        public bool hasBoost { get; private set; } = false;
        public float stamina { get; private set; }
        
        // Score/Kills
        public bool hasPlayerInfo { get; private set; } = false;
        public int score { get; private set; }
        public int kills { get; private set; }
        
        // Collision
        public bool hasCollision { get; private set; } = false;
        public float collisionSize { get; private set; }
        public float intangibility { get; private set; }

        // Only the milliseconds are used/serialized
        public TimeSpan updateWindow { get; private set; } = TimeSpan.Zero;

        public override byte[] serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(base.serialize());
            data.AddRange(BitConverter.GetBytes(id));

            data.AddRange(BitConverter.GetBytes(hasPosition));
            if (hasPosition)
            {
                data.AddRange(BitConverter.GetBytes(segments.Count));
                for (int i = 0; i < segments.Count; i++)
                {
                    var segment = segments[i];
                    data.AddRange(BitConverter.GetBytes(segment.X));
                    data.AddRange(BitConverter.GetBytes(segment.Y));
                }
            }
            
            data.AddRange(BitConverter.GetBytes(hasMovement));
            if (hasMovement)
            {
                data.AddRange(BitConverter.GetBytes(facing));
            }
            
            data.AddRange(BitConverter.GetBytes(hasBoost));
            if (hasBoost)
            {
                data.AddRange(BitConverter.GetBytes(stamina));
            }
            
            data.AddRange(BitConverter.GetBytes(hasPlayerInfo));
            if (hasPlayerInfo)
            {
                data.AddRange(BitConverter.GetBytes(score));
                data.AddRange(BitConverter.GetBytes(kills));
            }
            
            data.AddRange(BitConverter.GetBytes(hasCollision));
            if (hasCollision)
            {
                data.AddRange(BitConverter.GetBytes(collisionSize));
                data.AddRange(BitConverter.GetBytes(intangibility));
            }

            data.AddRange(BitConverter.GetBytes(updateWindow.Milliseconds));

            return data.ToArray();
        }

        public override int parse(byte[] data)
        {
            var offset = base.parse(data);

            id = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);

            hasPosition = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasPosition)
            {
                var howMany = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                for (var i = 0; i < howMany; i++)
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
                facing = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
            }

            hasBoost = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasBoost)
            {
                stamina = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
            }

            hasPlayerInfo = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasPlayerInfo)
            {
                score = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
                kills = BitConverter.ToInt32(data, offset);
                offset += sizeof(Int32);
            }

            hasCollision = BitConverter.ToBoolean(data, offset);
            offset += sizeof(bool);
            if (hasCollision)
            {
                collisionSize = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
                intangibility = BitConverter.ToSingle(data, offset);
                offset += sizeof(Single);
            }

            updateWindow = new TimeSpan(0, 0, 0, 0, BitConverter.ToInt32(data, offset));
            offset += sizeof(Int32);

            return offset;
        }
}