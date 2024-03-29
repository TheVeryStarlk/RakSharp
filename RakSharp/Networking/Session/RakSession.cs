﻿using System.Net;
using RakSharp.Packets.Offline;

namespace RakSharp.Networking.Session;

/// <summary>
/// Stores options that controls different <see cref="RakSession"/> operations.
/// </summary>
public sealed class RakSessionOptions
{
    /// <summary>
    /// The <see cref="IPEndPoint"/> to connect to.
    /// </summary>
    public required IPEndPoint RemoteEndPoint { get; init; }

    /// <summary>
    /// Specifies for how long a <see cref="RakSession"/> operation should be running before it times out.
    /// </summary>
    public required TimeSpan TimeOut { get; init; }
}

/// <summary>
/// Provides ways to deal with RakNet's offline state, as sessions.
/// </summary>
public static class RakSession
{
    /// <summary>
    /// Tries to ping a RakNet <see cref="IPEndPoint"/>.
    /// </summary>
    /// <param name="options">The provided <see cref="RakSessionOptions"/> to control the <see cref="RakSession"/> ping operation.</param>
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
        using var client = await RakClient.ConnectAsync(options.RemoteEndPoint);

        await client.WriteAsync(
            new UnconnectedPingPacket
            {
                Time = DateTime.UtcNow.Millisecond,
                Client = client.Identifier
            },
            timeOutSource.Token);

        var message = await client.ReadAsync(timeOutSource.Token);
        var pong = message.As<UnconnectedPongPacket>();

        return new StatusResponse(pong.Message);
    }
}