using System.Diagnostics;
using RakSharp.Packets;
using RakSharp.Packets.Online;
using RakSharp.Packets.Online.FrameSet;

namespace RakSharp.Networking.Connection;

internal sealed class RakConnectionTransport(RakClient client, CancellationToken token)
{
    private readonly HashSet<(int SequenceNumber, Memory<byte> Memory)> unacknowledgedMessages = [];

    private int sequenceNumber;

    public async Task<Message[]> ReadAsync()
    {
        var message = await client.ReadAsync(token);

        switch (message.Identifier)
        {
            case 0x80:
                var frameSet = message.As<FrameSetPacket>();

                await client.WriteAsync(
                    new AcknowledgementPacket
                    {
                        Records =
                        [
                            (frameSet.SequenceNumber, frameSet.SequenceNumber)
                        ]
                    },
                    token);

                var messages = new Message[frameSet.Frames.Length];

                for (var index = 0; index < messages.Length; index++)
                {
                    messages[index] = new Message(
                        frameSet.Frames[index].Memory.Span[0],
                        frameSet.Frames[index].Memory);
                }

                return messages;

            case 0xC0:
                var acknowledgement = message.As<AcknowledgementPacket>();

                foreach (var record in acknowledgement.Records)
                {
                    if (record.Start == record.End)
                    {
                        unacknowledgedMessages.RemoveWhere(
                            predicate => predicate.SequenceNumber == record.Start);
                    }
                    else
                    {
                        for (var number = record.Start; number <= record.End; number++)
                        {
                            unacknowledgedMessages.RemoveWhere(
                                predicate => predicate.SequenceNumber == number);
                        }
                    }
                }

                foreach (var unacknowledged in unacknowledgedMessages)
                {
                    await WriteAsync(unacknowledged.SequenceNumber, unacknowledged.Memory, Reliability.Unreliable);
                }

                break;
        }


        return Array.Empty<Message>();
    }

    public async Task WriteAsync<T>(T packet, Reliability reliability)
        where T : IOutgoingPacket
    {
        Debug.Assert(reliability is Reliability.Unreliable);

        var memory = new byte[RakNet.MaximumTransmissionUnit].AsMemory();
        memory = memory[..packet.Write(memory)];

        sequenceNumber++;
        unacknowledgedMessages.Add((sequenceNumber, memory));
        await WriteAsync(sequenceNumber, memory, reliability);
    }

    private async Task WriteAsync(int number, Memory<byte> memory, Reliability reliability)
    {
        await client.WriteAsync(
            new FrameSetPacket
            {
                SequenceNumber = number,
                Frames =
                [
                    new Frame
                    {
                        Flag = new UnreliableFlag(),
                        Memory = memory
                    }
                ]
            },
            token);
    }
}