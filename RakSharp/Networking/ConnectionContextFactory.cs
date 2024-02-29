using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Logging.Abstractions;

namespace RakSharp.Networking;

internal static class ConnectionContextFactory
{
    public static async Task<(ConnectionContext Context, SocketConnectionContextFactory Factory)>
        ConnectAsync(IPEndPoint endPoint)
    {
        var factory = new SocketConnectionContextFactory(
            new SocketConnectionFactoryOptions(),
            NullLogger.Instance);

        var socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        await socket.ConnectAsync(endPoint);
        return (factory.Create(socket), factory);
    }
}