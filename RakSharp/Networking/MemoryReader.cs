using System.Buffers.Binary;
using System.Net;
using System.Text;

namespace RakSharp.Networking;

internal ref struct MemoryReader(ReadOnlyMemory<byte> memory)
{
    public bool IsAtEnd => position >= span.Length;

    private ReadOnlySpan<byte> span = memory.Span;
    private int position;

    public ReadOnlyMemory<byte> Read(int length)
    {
        return memory[position..(position += length)];
    }

    public byte ReadByte()
    {
        return span[position++];
    }

    public bool ReadBoolean()
    {
        return ReadByte() is 1;
    }

    public short ReadShort()
    {
        return BinaryPrimitives.ReadInt16BigEndian(
            span[position..(position += sizeof(short))]);
    }

    public ushort ReadUnsignedShort()
    {
        return BinaryPrimitives.ReadUInt16BigEndian(
            span[position..(position += sizeof(ushort))]);
    }

    public long ReadLong()
    {
        return BinaryPrimitives.ReadInt64BigEndian(
            span[position..(position += sizeof(long))]);
    }

    public int ReadSmallInteger()
    {
        var value = ReadByte() | ReadByte() << 8 | ReadByte() << 16;

        return BitConverter.IsLittleEndian
            ? value
            : BinaryPrimitives.ReverseEndianness(value);
    }

    public IPEndPoint ReadEndPoint()
    {
        var family = ReadByte();

        IPAddress address;
        int port;

        switch (family)
        {
            case 4:
            {
                address = IPAddress.Parse($"{(byte) ~ReadByte()}."
                                          + $"{(byte) ~ReadByte()}."
                                          + $"{(byte) ~ReadByte()}."
                                          + $"{(byte) ~ReadByte()}");

                port = ReadUnsignedShort();
                break;
            }

            case 6:
            {
                position += sizeof(short);
                port = ReadUnsignedShort();
                position += sizeof(long);
                address = new IPAddress(Read(16).Span);
                break;
            }

            default:
                throw new InvalidOperationException("Unknown address family.");
        }

        return new IPEndPoint(address, port);
    }

    public string ReadVariableString()
    {
        var length = ReadUnsignedShort();
        return Encoding.UTF8.GetString(span[position..(position += length)]);
    }

    public void ReadMagic()
    {
        ReadOnlySpan<byte> magic = stackalloc byte[]
        {
            0x00, 0xFF, 0xFF, 0x00, 0xFE, 0xFE, 0xFE, 0xFE, 0xFD, 0xFD, 0xFD, 0xFD, 0x12, 0x34, 0x56, 0x78
        };

        if (!span[position..(position += magic.Length)].SequenceEqual(magic))
        {
            throw new InvalidOperationException("Invalid magic.");
        }
    }
}