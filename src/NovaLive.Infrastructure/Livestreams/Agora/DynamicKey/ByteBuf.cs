namespace NovaLive.Infrastructure.Livestreams.Agora.DynamicKey;

public class ByteBuf
{
    private readonly ByteBuffer _buffer;

    public ByteBuf()
    {
        _buffer = new ByteBuffer();
    }

    public ByteBuf(byte[] source)
    {
        _buffer = new ByteBuffer();
        _buffer.PushByteArray(source);
    }

    public byte[] AsBytes()
    {
        return _buffer.ToByteArray();
    }

    public ByteBuf Put(ushort v)
    {
        _buffer.PushUInt16(v);
        return this;
    }

    public ByteBuf Put(uint v)
    {
        _buffer.PushInt(v);
        return this;
    }

    public ByteBuf Put(byte[] v)
    {
        Put((ushort)v.Length);
        _buffer.PushByteArray(v);
        return this;
    }

    public ByteBuf Copy(byte[] v)
    {
        _buffer.PushByteArray(v);
        return this;
    }

    public ByteBuf PutIntMap(SortedDictionary<ushort, uint> extra)
    {
        Put((ushort)extra.Count);
        foreach (var item in extra)
        {
            Put(item.Key);
            Put(item.Value);
        }
        return this;
    }

    public ushort ReadShort()
    {
        return _buffer.PopUInt16();
    }

    public uint ReadInt()
    {
        return _buffer.PopUInt();
    }

    public byte[] ReadBytes()
    {
        ushort length = ReadShort();
        return _buffer.PopByteArray(length);
    }

    public SortedDictionary<ushort, uint> ReadIntMap()
    {
        var map = new SortedDictionary<ushort, uint>();
        ushort length = ReadShort();

        for (short i = 0; i < length; ++i)
        {
            ushort k = ReadShort();
            uint v = ReadInt();
            map[k] = v;
        }

        return map;
    }
}
