namespace RakSharp.Networking.Packets.Online;

internal sealed class ConnectedPongPacket : IIngoingPacket<ConnectedPongPacket>, IOutgoingPacket
{
    public static byte Identifier => 0x03;

    public required long Ping { get; init; }

    public required long Pong { get; init; }

    public static ConnectedPongPacket Read(MemoryReader reader)
    {
        return new ConnectedPongPacket
        {
            Ping = reader.ReadLong(),
            Pong = reader.ReadLong()
        };
    }

    public void Write(ref MemoryWriter writer)
    {
        writer.WriteLong(Ping);
        writer.WriteLong(Pong);
    }
}