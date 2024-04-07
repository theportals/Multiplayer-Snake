namespace Shared.Messages;

public class RemoveEntity : Message
{
    public uint removeId { get; private set; }
    public Reasons reason { get; private set; }
    public uint? reasonId { get; private set; }
    
    public RemoveEntity() : base(Type.RemoveEntity)
    {
    }

    public RemoveEntity(uint removeId, Reasons reason, uint? reasonId) : base(Type.RemoveEntity)
    {
        this.removeId = removeId;
        this.reason = reason;
        this.reasonId = reasonId;
    }

    public override byte[] serialize()
    {
        var data = new List<byte>();
        
        data.AddRange(base.serialize());
        data.AddRange(BitConverter.GetBytes(removeId));
        data.AddRange(BitConverter.GetBytes((UInt16)reason));
        data.AddRange(BitConverter.GetBytes(reasonId.HasValue));
        if (reasonId.HasValue)
        {
            data.AddRange(BitConverter.GetBytes(reasonId.Value));
        }

        return data.ToArray();
    }

    public override int parse(byte[] data)
    {
        int offset = base.parse(data);

        removeId = BitConverter.ToUInt32(data, offset);
        offset += sizeof(UInt32);

        reason = (Reasons)BitConverter.ToUInt16(data, offset);
        offset += sizeof(UInt16);

        var hasReasonId = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasReasonId)
        {
            reasonId = BitConverter.ToUInt32(data, offset);
            offset += sizeof(UInt32);
        }

        return offset;
    }

    public enum Reasons
    {
        FOOD_EXPIRED,
        FOOD_CONSUMED,
        PLAYER_DIED,
        PLAYER_RESPAWNED,
        PLAYER_DISCONNECT
    }
}