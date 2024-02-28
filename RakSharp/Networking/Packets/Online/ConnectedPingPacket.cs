namespace RakSharp.Networking.Packets.Online;

internal sealed class ConnectedPingPacket : IIngoingPacket<ConnectedPingPacket>, IOutgoingPacket
{
    public static byte Identifier => 0x00;

    public required long Time { get; init; }

    public static ConnectedPingPacket Read(MemoryReader reader)
    {
        return new ConnectedPingPacket
        {
            Time = reader.ReadLong()
        };
    }

    public void Write(ref MemoryWriter writer)
    {
        writer.WriteLong(Time);
    }
}