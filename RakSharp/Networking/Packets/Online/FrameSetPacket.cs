namespace RakSharp.Networking.Packets.Online;

internal sealed class FrameSetPacket : IIngoingPacket<FrameSetPacket>, IOutgoingPacket
{
    public static byte Identifier => 0x80;

    public required int SequenceNumber { get; init; }

    public required Frame[] Frames { get; init; }

    public static FrameSetPacket Read(MemoryReader reader)
    {
        var sequenceNumber = reader.ReadSmallInteger();
        var frames = new HashSet<Frame>();

        while (!reader.IsAtEnd)
        {
            var (reliability, isSplit) = FlagBase.Read(reader.ReadByte());
            var length = reader.ReadUnsignedShort();

            FlagBase flag = reliability switch
            {
                Reliability.Unreliable => new UnreliableFlag(),
                Reliability.Reliable => new ReliableFlag
                {
                    ReliableFrameIndex = reader.ReadSmallInteger()
                },
                Reliability.ReliableOrdered => new ReliableOrderedFlag
                {
                    ReliableFrameIndex = reader.ReadSmallInteger(),
                    OrderedFrameIndex = reader.ReadSmallInteger(),
                    OrderChannel = reader.ReadByte()
                },
                _ => throw new ArgumentOutOfRangeException(nameof(reliability), reliability, "Unknown flag.")
            };

            flag.IsSplit = isSplit;
            var memory = reader.Read(length / 8).ToArray();

            frames.Add(new Frame
            {
                Flag = flag,
                Memory = memory
            });
        }

        return new FrameSetPacket
        {
            SequenceNumber = sequenceNumber,
            Frames = frames.ToArray()
        };
    }

    public void Write(ref MemoryWriter writer)
    {
        writer.WriteSmallInteger(SequenceNumber);

        foreach (var frame in Frames)
        {
            writer.WriteByte(FlagBase.Write(frame.Flag.Reliability, frame.Flag.IsSplit));
            writer.WriteUnsignedShort((ushort) (frame.Memory.Length * 8));

            switch (frame.Flag.Reliability)
            {
                case Reliability.Unreliable:
                    break;

                case Reliability.Reliable when frame.Flag is ReliableFlag reliable:
                    writer.WriteSmallInteger(reliable.ReliableFrameIndex);
                    break;

                case Reliability.ReliableOrdered when frame.Flag is ReliableOrderedFlag reliableOrdered:
                    writer.WriteSmallInteger(reliableOrdered.ReliableFrameIndex);
                    writer.WriteSmallInteger(reliableOrdered.OrderedFrameIndex);
                    writer.WriteByte(reliableOrdered.OrderChannel);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(frame.Flag), frame.Flag, "Unknown flag.");
            }

            writer.Write(frame.Memory);
        }
    }
}

internal sealed class Frame
{
    public required FlagBase Flag { get; init; }

    public required Memory<byte> Memory { get; init; }
}

internal abstract class FlagBase
{
    public abstract Reliability Reliability { get; }

    public abstract bool IsSplit { get; set; }

    private const int Index = 0x05;
    private const byte Type = 0b11100000;
    private const byte Split = 0b00010000;

    public static (Reliability reliability, bool isSplit) Read(byte value)
    {
        return ((Reliability) ((value & Type) >> Index), (value & Split) > 0);
    }

    public static byte Write(Reliability reliability, bool isSplit)
    {
        return (byte) ((byte) reliability << Index | (isSplit ? Split : 0));
    }
}

internal class UnreliableFlag : FlagBase
{
    public override Reliability Reliability => Reliability.Unreliable;

    public override bool IsSplit { get; set; }
}

internal class ReliableFlag : UnreliableFlag
{
    public new Reliability Reliability => Reliability.Reliable;

    public required int ReliableFrameIndex { get; init; }
}

internal sealed class ReliableOrderedFlag : ReliableFlag
{
    public new Reliability Reliability => Reliability.ReliableOrdered;

    public required int OrderedFrameIndex { get; init; }

    public required byte OrderChannel { get; init; }
}

internal enum Reliability
{
    Unreliable,
    UnreliableSequenced,
    Reliable,
    ReliableOrdered
}