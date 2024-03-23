namespace Shared.Messages;

public class Input : Message
{
    public uint entityId { get; private set; }
    public List<Components.Input.Type> inputs { get; private set; }
    public TimeSpan elapsedTime { get; private set; }
    public Input(uint entityId, List<Components.Input.Type> inputs, TimeSpan elapsedTime) : base(Type.Input)
    {
        this.entityId = entityId;
        this.inputs = inputs;
        this.elapsedTime = elapsedTime;
    }

    public Input() : base(Type.Input)
    {
        elapsedTime = TimeSpan.Zero;
        inputs = new List<Components.Input.Type>();
    }

    public override byte[] serialize()
    {
        var data = new List<byte>();

        data.AddRange(base.serialize());
        data.AddRange(BitConverter.GetBytes(entityId));
        
        data.AddRange(BitConverter.GetBytes(inputs.Count));
        foreach (var input in inputs)
        {
            data.AddRange(BitConverter.GetBytes((UInt16)input));
        }
        
        data.AddRange(BitConverter.GetBytes(elapsedTime.Milliseconds));

        return data.ToArray();
    }

    public override int parse(byte[] data)
    {
        var offset = base.parse(data);

        entityId = BitConverter.ToUInt32(data, offset);
        offset += sizeof(UInt32);

        var howMany = BitConverter.ToInt32(data, offset);
        offset += sizeof(UInt32);

        for (var i = 0; i < howMany; i++)
        {
            var input = (Components.Input.Type)BitConverter.ToUInt16(data, offset);
            offset += sizeof(UInt16);
            inputs.Add(input);
        }

        elapsedTime = new TimeSpan( 0, 0, 0, 0, BitConverter.ToInt32(data, offset));
        offset += sizeof(Int32);

        return offset;
    }
}