using System.Runtime.Loader;
using System.Text;

namespace Shared.Messages;

public class Join : Message
{
    public string playerName { get; private set; }
    public Join() : base(Type.Join)
    {
    }

    public Join(string playerName) : base(Type.Join)
    {
        // Console.WriteLine($"Constructing packet with name {playerName}");
        this.playerName = playerName;
    }
    
    public override byte[] serialize()
    {
        var data = new List<Byte>();
        data.AddRange(base.serialize());
        data.AddRange(BitConverter.GetBytes(playerName.Length));
        data.AddRange(Encoding.UTF8.GetBytes(playerName));

        return data.ToArray();
    }
    
    public override int parse(byte[] data)
    {
        var offset = base.parse(data);

        var nameSize = BitConverter.ToInt32(data, offset);
        offset += sizeof(Int32);
        if (nameSize > 0) playerName = Encoding.UTF8.GetString(data, offset, nameSize);
        offset += nameSize;

        return offset;
    }
}