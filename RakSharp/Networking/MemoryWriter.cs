using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace RakSharp.Networking;

internal ref struct MemoryWriter(Memory<byte> memory)
{
    public int Position { get; private set; }

    private Span<byte> span = memory.Span;

    public void Write(in Memory<byte> source)
    {
        source.CopyTo(memory[Position..(Position += source.Length)]);
    }

    public void WriteByte(byte value)
    {
        span[Position++] = value;
    }

    public void WriteBoolean(bool value)
    {
        WriteByte((byte) (value ? 1 : 0));
    }

    public void WriteShort(short value)
    {
        BinaryPrimitives.WriteInt16BigEndian(
            span[Position..(Position += sizeof(short))],
            value);
    }

    public void WriteUnsignedShort(ushort value)
    {
        BinaryPrimitives.WriteUInt16BigEndian(
            span[Position..(Position += sizeof(ushort))],
            value);
    }

    public void WriteLong(long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(
            span[Position..(Position += sizeof(long))],
            value);
    }

    public void WriteSmallInteger(int value)
    {
        if (!BitConverter.IsLittleEndian)
        {
            value = BinaryPrimitives.ReverseEndianness(value);
        }

        WriteByte((byte) value);
        WriteByte((byte) (value >> 8));
        WriteByte((byte) (value >> 16));
    }

    public void WriteEndPoint(IPEndPoint endPoint)
    {
        if (endPoint.AddressFamily is AddressFamily.InterNetwork)
        {
            WriteByte(4);

            var parts = endPoint.Address.ToString().Split('.');

            foreach (var part in parts)
            {
                span[Position++] = (byte) ~byte.Parse(part);
            }

            WriteUnsignedShort((ushort) endPoint.Port);
        }
        else
        {
            throw new InvalidOperationException("Unknown address family.");
        }
    }

    public void WriteMagic()
    {
        ReadOnlySpan<byte> magic = stackalloc byte[]
        {
            0x00, 0xFF, 0xFF, 0x00, 0xFE, 0xFE, 0xFE, 0xFE, 0xFD, 0xFD, 0xFD, 0xFD, 0x12, 0x34, 0x56, 0x78
        };

        magic.CopyTo(span[Position..(Position += magic.Length)]);
    }
}