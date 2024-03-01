using System.Diagnostics;
using System.IO.Pipelines;
using CommunityToolkit.HighPerformance.Buffers;
using RakSharp.Packets;
using RakSharp.Packets.Online.FrameSet;

namespace RakSharp.Networking.Connection;

internal sealed class RakConnectionTransport(IDuplexPipe duplexPipe, CancellationToken token)
{
    private int sequenceNumber;

    public async Task<Message[]> ReadAsync()
    {
        var message = await duplexPipe.ReadAsync(token);
        var frameSet = message.As<FrameSetPacket>();

        var messages = new Message[frameSet.Frames.Length];
        for (var index = 0; index < messages.Length; index++)
        {
            messages[index] = new Message(
                frameSet.Frames[index].Memory.Span[1],
                frameSet.Frames[index].Memory[1..]);
        }

        return messages;
    }

    public async Task WriteAsync<T>(IOutgoingPacket packet, Reliability reliability)
        where T : IOutgoingPacket
    {
        Debug.Assert(reliability is Reliability.Unreliable);

        using var owner = MemoryOwner<byte>.Allocate(RakSharp.MaximumTransmissionUnit);
        using var slicedOwner = owner[..packet.Write<T>(owner.Memory)];

        await duplexPipe.WriteAsync(
            new FrameSetPacket
            {
                SequenceNumber = sequenceNumber++,
                Frames =
                [
                    new Frame
                    {
                        Flag = new UnreliableFlag(),
                        Memory = slicedOwner.Memory
                    }
                ]
            },
            token);
    }
}