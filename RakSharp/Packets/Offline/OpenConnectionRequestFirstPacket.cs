namespace RakSharp.Packets.Offline;

internal sealed class OpenConnectionRequestFirstPacket : IOutgoingPacket
{
    public static byte Identifier => 0x05;

    public required byte ProtocolVersion { get; init; }

    public required short MaximumTransmissionUnit { get; init; }

    public void Write(ref MemoryWriter writer)
    {
        writer.WriteMagic();
        writer.WriteByte(ProtocolVersion);
        writer.Write(new byte[MaximumTransmissionUnit - writer.Position]);
    }
}