namespace RakSharp.Networking.Packets.Online;

internal sealed class NegativeAcknowledgementPacket : IOutgoingPacket
{
    public static byte Identifier => 0xA0;

    public void Write(ref MemoryWriter writer)
    {
        throw new NotImplementedException();
    }
}