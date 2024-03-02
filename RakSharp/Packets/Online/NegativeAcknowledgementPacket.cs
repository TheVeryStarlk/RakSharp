namespace RakSharp.Packets.Online;

internal sealed class NegativeAcknowledgementPacket : IIngoingPacket<NegativeAcknowledgementPacket>, IOutgoingPacket
{
    public static byte Identifier => 0xA0;

    public required (int Start, int End)[] Records { get; init; }

    public static NegativeAcknowledgementPacket Read(MemoryReader reader)
    {
        var records = new (int Start, int End)[reader.ReadShort()];

        for (var index = 0; index < records.Length; index++)
        {
            if (reader.ReadBoolean())
            {
                var sequenceNumber = reader.ReadSmallInteger();
                records[index] = (sequenceNumber, sequenceNumber);
            }
            else
            {
                records[index] = (reader.ReadSmallInteger(), reader.ReadSmallInteger());
            }
        }

        return new NegativeAcknowledgementPacket
        {
            Records = records
        };
    }

    public void Write(ref MemoryWriter writer)
    {
        writer.WriteShort((short) Records.Length);

        foreach (var (start, end) in Records)
        {
            if (start == end)
            {
                writer.WriteBoolean(true);
                writer.WriteSmallInteger(start);
            }
            else
            {
                writer.WriteSmallInteger(start);
                writer.WriteSmallInteger(end);
            }
        }
    }
}