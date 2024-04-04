namespace Shared.Messages;

public class RemoveEntity : Message
{
    public uint id { get; private set; }
    
    public RemoveEntity() : base(Type.RemoveEntity)
    {
    }

    public RemoveEntity(uint id) : base(Type.RemoveEntity)
    {
        this.id = id;
    }

    public override byte[] serialize()
    {
        var data = new List<byte>();
        
        data.AddRange(base.serialize());
        data.AddRange(BitConverter.GetBytes(id));

        return data.ToArray();
    }

    public override int parse(byte[] data)
    {
        int offset = base.parse(data);

        id = BitConverter.ToUInt32(data, offset);
        offset += sizeof(UInt32);

        return offset;
    }
}