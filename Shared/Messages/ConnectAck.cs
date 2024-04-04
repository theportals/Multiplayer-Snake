namespace Shared.Messages;

public class ConnectAck : Message
{
    public ConnectAck() : base(Type.ConnectAck)
    {
    }

    public override byte[] serialize()
    {
        return base.serialize();
    }

    public override int parse(byte[] data)
    {
        return base.parse(data);
    }
}