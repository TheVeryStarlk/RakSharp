using System.Net;

namespace RakSharp.Networking.Packets.Offline;

internal sealed class OpenConnectionRequestSecondPacket : IOutgoingPacket
{
    public static byte Identifier => 0x07;

    public required IPEndPoint Server { get; init; }

    public required short MaximumTransmissionUnit { get; init; }

    public required long Client { get; init; }

    public void Write(ref MemoryWriter writer)
    {
        writer.WriteMagic();
        writer.WriteEndPoint(Server);
        writer.WriteShort(MaximumTransmissionUnit);
        writer.WriteLong(Client);
    }
}