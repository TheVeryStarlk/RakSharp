namespace RakSharp.Packets.Online;

internal sealed class ConnectionRequestPacket : IOutgoingPacket
{
    public static byte Identifier => 0x09;

    public required long Client { get; init; }

    public required long Time { get; init; }

    public required bool UseSecurity { get; init; }

    public void Write(ref MemoryWriter writer)
    {
        writer.WriteLong(Client);
        writer.WriteLong(Time);
        writer.WriteBoolean(UseSecurity);
    }
}