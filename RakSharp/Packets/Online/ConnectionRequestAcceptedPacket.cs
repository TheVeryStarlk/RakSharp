using System.Net;

namespace RakSharp.Packets.Online;

internal sealed class ConnectionRequestAcceptedPacket : IIngoingPacket<ConnectionRequestAcceptedPacket>
{
    public static byte Identifier => 0x10;

    public required IPEndPoint Client { get; init; }

    public required short SystemIndex { get; init; }

    public required IPEndPoint[] InternalAddresses { get; init; }

    public required long Request { get; init; }

    public required long Time { get; init; }

    public static ConnectionRequestAcceptedPacket Read(MemoryReader reader)
    {
        return new ConnectionRequestAcceptedPacket
        {
            Client = reader.ReadEndPoint(),
            SystemIndex = reader.ReadShort(),
            InternalAddresses = Enumerable.Repeat(reader.ReadEndPoint(), 10).ToArray(),
            Request = reader.ReadLong(),
            Time = reader.ReadLong()
        };
    }
}