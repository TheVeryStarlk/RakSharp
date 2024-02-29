namespace RakSharp.Packets.Offline;

internal sealed class OpenConnectionReplyFirstPacket : IIngoingPacket<OpenConnectionReplyFirstPacket>
{
    public static byte Identifier => 0x06;

    public required long Server { get; init; }

    public required bool UseSecurity { get; init; }

    public required short MaximumTransmissionUnit { get; init; }

    public static OpenConnectionReplyFirstPacket Read(MemoryReader reader)
    {
        reader.ReadMagic();

        return new OpenConnectionReplyFirstPacket
        {
            Server = reader.ReadLong(),
            UseSecurity = reader.ReadBoolean(),
            MaximumTransmissionUnit = reader.ReadShort()
        };
    }
}