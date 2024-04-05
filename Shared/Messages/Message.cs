namespace Shared.Messages;

public abstract class Message
{
    public Type type { get; private set; }
    public uint? messageId { get; set; }
    
    public Message(Type type)
    {
        this.type = type;
    }

    public virtual byte[] serialize()
    {
        List<byte> data = new List<byte>();
        
        data.AddRange(BitConverter.GetBytes(messageId.HasValue));
        if (messageId.HasValue)
        {
            data.AddRange(BitConverter.GetBytes(messageId.Value));
        }

        return data.ToArray();
    }

    public virtual int parse(byte[] data)
    {
        if (data.Length < 1) return 0;
        var offset = 0;

        var hasValue = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (hasValue)
        {
            messageId = BitConverter.ToUInt32(data, offset);
            offset += sizeof(uint);
        }

        return offset;
    }
}