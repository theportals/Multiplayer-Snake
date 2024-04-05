namespace Shared.Messages;

public class Input : Message
{
    public uint entityId { get; private set; }
    
    public float newFacing { get; private set; }
    public bool boosting { get; private set; }
    public bool hasStamina { get; private set; }
    public TimeSpan elapsedTime { get; private set; }
    
    public Input(uint entityId, float newFacing, bool boosting, bool hasStamina, TimeSpan elapsedTime) : base(Type.Input)
    {
        this.entityId = entityId;
        this.newFacing = newFacing;
        this.boosting = boosting;
        this.hasStamina = hasStamina;
        this.elapsedTime = elapsedTime;
    }

    public Input() : base(Type.Input)
    {
        elapsedTime = TimeSpan.Zero;
    }

    public override byte[] serialize()
    {
        var data = new List<byte>();

        data.AddRange(base.serialize());
        data.AddRange(BitConverter.GetBytes(entityId));
        
        data.AddRange(BitConverter.GetBytes(newFacing));
        data.AddRange(BitConverter.GetBytes(boosting));
        data.AddRange(BitConverter.GetBytes(hasStamina));
        
        data.AddRange(BitConverter.GetBytes(elapsedTime.Milliseconds));

        return data.ToArray();
    }

    public override int parse(byte[] data)
    {
        var offset = base.parse(data);

        entityId = BitConverter.ToUInt32(data, offset);
        offset += sizeof(UInt32);

        newFacing = BitConverter.ToSingle(data, offset);
        offset += sizeof(Single);
        boosting = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        hasStamina = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);

        elapsedTime = new TimeSpan( 0, 0, 0, 0, BitConverter.ToInt32(data, offset));
        offset += sizeof(Int32);

        return offset;
    }
}