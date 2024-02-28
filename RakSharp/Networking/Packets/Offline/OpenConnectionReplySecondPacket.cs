using System.Net;

namespace RakSharp.Networking.Packets.Offline;

internal sealed class OpenConnectionReplySecondPacket : IIngoingPacket<OpenConnectionReplySecondPacket>
{
    public static byte Identifier => 0x08;

    public required long Server { get; init; }

    public required IPEndPoint Client { get; init; }

    public required short MaximumTransmissionUnit { get; init; }

    public required bool HasEncryption { get; init; }

    public static OpenConnectionReplySecondPacket Read(MemoryReader reader)
    {
        reader.ReadMagic();

        return new OpenConnectionReplySecondPacket
        {
            Server = reader.ReadLong(),
            Client = reader.ReadEndPoint(),
            MaximumTransmissionUnit = reader.ReadShort(),
            HasEncryption = reader.ReadBoolean()
        };
    }
}