using System.Net;
using RakSharp.Networking;
using RakSharp.Networking.Packets.Offline;

namespace RakSharp.Session;

/// <summary>
/// Stores options that control the <see cref="RakSession"/>.
/// </summary>
public sealed class RakSessionOptions
{
    /// <summary>
    /// The <see cref="IPEndPoint"/> to connect to.
    /// </summary>
    public required IPEndPoint RemoteEndPoint { get; set; }

    /// <summary>
    /// Specifies for how long the <see cref="RakSession"/> should attempt to connect before it times out.
    /// </summary>
    public required TimeSpan TimeOut { get; set; }
}

/// <summary>
/// Provides ways to deal with offline RakNet sessions.
/// </summary>
public static class RakSession
{
    /// <summary>
    /// Tries to ping a RakNet server.
    /// </summary>
    /// <param name="options">The provided <see cref="RakSessionOptions"/> to control the <see cref="RakSession"/>.</param>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous read operation,
    /// which wraps the <see cref="StatusResponse"/> containing the contents of the pong.
    /// </returns>
    public static async Task<StatusResponse> PingAsync(RakSessionOptions options, CancellationToken token = default)
    {
        using var timeOutSource = new CancellationTokenSource();
        timeOutSource.CancelAfter(options.TimeOut);

        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, timeOutSource.Token);
        var connection = await ConnectionContextFactory.ConnectAsync(options.RemoteEndPoint);

        await connection.Context.Transport.WriteAsync(new UnconnectedPingPacket
            {
                Time = DateTime.UtcNow.Millisecond,
                Client = Random.Shared.NextInt64()
            },
            timeOutSource.Token);

        var message = await connection.Context.Transport.ReadAsync(timeOutSource.Token);
        var pong = message.As<UnconnectedPongPacket>();

        await connection.Context.DisposeAsync();
        connection.Factory.Dispose();

        return new StatusResponse(pong.Message);
    }
}