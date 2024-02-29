using System.Buffers;
using System.IO.Pipelines;

namespace RakSharp.Packets;

internal static class DuplexPipeExtensions
{
    public static async Task<Message> ReadAsync(
        this IDuplexPipe duplexPipe,
        CancellationToken cancellationToken)
    {
        var result = await duplexPipe.Input.ReadAsync(cancellationToken);
        var buffer = result.Buffer;
        duplexPipe.Input.AdvanceTo(buffer.End);

        var array = buffer.ToArray();
        return new Message(array[0], array.AsMemory()[1..]);
    }

    public static async Task WriteAsync<T>(
        this IDuplexPipe duplexPipe,
        T packet,
        CancellationToken cancellationToken) where T : IOutgoingPacket
    {
        var memory = duplexPipe.Output.GetMemory();
        duplexPipe.Output.Advance(Write(packet, memory));
        await duplexPipe.Output.FlushAsync(cancellationToken);
        return;

        static int Write(IOutgoingPacket packet, Memory<byte> memory)
        {
            var writer = new MemoryWriter(memory);
            writer.WriteByte(T.Identifier);
            packet.Write(ref writer);
            return writer.Position;
        }
    }
}

internal sealed record Message(int Identifier, ReadOnlyMemory<byte> Memory)
{
    public T As<T>() where T : IIngoingPacket<T>
    {
        if (T.Identifier != Identifier)
        {
            throw new ArgumentException($"Expected {T.Identifier} but got {Identifier} instead.");
        }

        var reader = new MemoryReader(Memory);
        return T.Read(reader);
    }
}