namespace RakSharp.Networking.Packets.Online;

internal sealed class DisconnectPacket : IOutgoingPacket
{
    public static byte Identifier => 0x15;

    public void Write(ref MemoryWriter writer)
    {
    }
}