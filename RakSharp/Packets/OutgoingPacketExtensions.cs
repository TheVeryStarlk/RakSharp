namespace RakSharp.Packets;

internal static class OutgoingPacketExtensions
{
    public static int Write<T>(this IOutgoingPacket packet, Memory<byte> memory)
        where T : IOutgoingPacket
    {
        var writer = new MemoryWriter(memory);
        writer.WriteByte(T.Identifier);
        packet.Write(ref writer);
        return writer.Position;
    }
}