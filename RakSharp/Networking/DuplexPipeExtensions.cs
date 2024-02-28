using System.Buffers;
using System.IO.Pipelines;
using RakSharp.Networking.Packets;

namespace RakSharp.Networking;

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

internal static class DuplexPipeExtensions
{
    public static async Task<Memory<byte>> ReadAsync(
        this IDuplexPipe duplexPipe,
        CancellationToken cancellationToken)
    {
        var result = await duplexPipe.Input.ReadAsync(cancellationToken);
        var buffer = result.Buffer;
        duplexPipe.Input.AdvanceTo(buffer.Start, buffer.End);
        return buffer.ToArray();
    }

    public static async Task WriteAsync<T>(
        this IDuplexPipe duplexPipe,
        IOutgoingPacket packet,
        CancellationToken cancellationToken) where T : IOutgoingPacket
    {
        static int Write(IOutgoingPacket packet, Memory<byte> memory)
        {
            var writer = new MemoryWriter(memory);
            writer.WriteByte(T.Identifier);
            packet.Write(ref writer);
            return writer.Position;
        }

        var memory = duplexPipe.Output.GetMemory();
        duplexPipe.Output.Advance(Write(packet, memory));
        await duplexPipe.Output.FlushAsync(cancellationToken);
    }
}