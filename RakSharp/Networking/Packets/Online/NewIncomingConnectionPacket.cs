using System.Net;

namespace RakSharp.Networking.Packets.Online;

internal sealed class NewIncomingConnectionPacket : IOutgoingPacket
{
    public static byte Identifier => 0x13;

    public required IPEndPoint Server { get; init; }

    public void Write(ref MemoryWriter writer)
    {
        writer.WriteEndPoint(Server);

        for (var count = 0; count < 20; count++)
        {
            writer.WriteEndPoint(Server);
        }

        writer.WriteLong(long.MaxValue);
        writer.WriteLong(long.MaxValue);
    }
}