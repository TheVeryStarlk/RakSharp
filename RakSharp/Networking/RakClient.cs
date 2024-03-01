using System.Net;
using System.Net.Sockets;
using RakSharp.Packets;

namespace RakSharp.Networking;

internal sealed class RakClient : IDisposable
{
    public long Identifier { get; } = Random.Shared.NextInt64();

    private readonly Socket socket;

    private RakClient(Socket socket)
    {
        this.socket = socket;
    }

    public static async Task<RakClient> ConnectAsync(IPEndPoint remoteEndPoint)
    {
        var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        await socket.ConnectAsync(remoteEndPoint);
        return new RakClient(socket);
    }

    public async Task<Message> ReadAsync(CancellationToken cancellationToken)
    {
        var memory = new byte[RakSharp.MaximumTransmissionUnit].AsMemory();
        memory = memory[..await socket.ReceiveAsync(memory, cancellationToken)];
        return new Message(memory.Span[0], memory);
    }

    public async Task WriteAsync<T>(T packet, CancellationToken cancellationToken) where T : IOutgoingPacket
    {
        var memory = new byte[RakSharp.MaximumTransmissionUnit].AsMemory();
        memory = memory[..packet.Write<T>(memory)];
        await socket.SendAsync(memory, cancellationToken);
    }

    public void Disconnect()
    {
        socket.Shutdown(SocketShutdown.Both);
    }

    public void Dispose()
    {
        socket.Dispose();
    }
}