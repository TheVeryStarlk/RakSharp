using System.Diagnostics;
using CommunityToolkit.HighPerformance.Buffers;
using RakSharp.Packets;
using RakSharp.Packets.Online.FrameSet;

namespace RakSharp.Networking.Connection;

internal sealed class RakConnectionTransport(RakClient client, CancellationToken token)
{
    private int sequenceNumber;

    public async Task<Message[]> ReadAsync()
    {
        var message = await client.ReadAsync(token);

        if (message.Identifier is 0x80)
        {
            var frameSet = message.As<FrameSetPacket>();
            var messages = new Message[frameSet.Frames.Length];

            for (var index = 0; index < messages.Length; index++)
            {
                messages[index] = new Message(
                    frameSet.Frames[index].Memory.Span[0],
                    frameSet.Frames[index].Memory);
            }

            return messages;
        }

        return Array.Empty<Message>();
    }

    public async Task WriteAsync<T>(IOutgoingPacket packet, Reliability reliability)
        where T : IOutgoingPacket
    {
        Debug.Assert(reliability is Reliability.Unreliable);

        using var owner = MemoryOwner<byte>.Allocate(RakSharp.MaximumTransmissionUnit);
        using var slicedOwner = owner[..packet.Write<T>(owner.Memory)];

        await client.WriteAsync(
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