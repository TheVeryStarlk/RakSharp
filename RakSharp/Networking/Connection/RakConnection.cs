﻿using System.Net;
using RakSharp.Networking.Session;
using RakSharp.Packets.Offline;
using RakSharp.Packets.Online;
using RakSharp.Packets.Online.FrameSet;

namespace RakSharp.Networking.Connection;

/// <inheritdoc />
public sealed class RakConnection : IRakConnection
{
    public IPEndPoint RemoteEndPoint { get; }

    public short MaximumTransmissionUnit { get; private set; }

    private RakConnectionState state = RakConnectionState.Connected;

    private readonly CancellationTokenSource source = new CancellationTokenSource();
    private readonly RakConnectionTransport transport;
    private readonly RakClient client;

    private RakConnection(RakClient client, IPEndPoint remoteEndPoint, short maximumTransmissionUnit)
    {
        this.client = client;
        transport = new RakConnectionTransport(client, source.Token);

        RemoteEndPoint = remoteEndPoint;
        MaximumTransmissionUnit = maximumTransmissionUnit;
    }

    /// <summary>
    /// Attempts ping the <see cref="IPEndPoint"/> then tries to initiate a <see cref="RakConnection"/>.
    /// </summary>
    /// <param name="options">The provided <see cref="RakConnectionOptions"/> to control the <see cref="RakConnection"/>.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous connect operation,
    /// which wraps the initiated <see cref="RakConnection"/>.
    /// </returns>
    /// <exception cref="NotImplementedException">The method is not implemented yet.</exception>
    public static async Task<IRakConnection> ConnectAsync(RakConnectionOptions options)
    {
        _ = await RakSession.PingAsync(new RakSessionOptions
        {
            RemoteEndPoint = options.RemoteEndPoint,
            TimeOut = options.TimeOut
        });

        var connection = new RakConnection(
            await RakClient.ConnectAsync(options.RemoteEndPoint),
            options.RemoteEndPoint,
            options.MaximumTransmissionUnit);

        _ = connection.StartAsync();
        return connection;
    }

    public Task<Memory<byte>> ReadAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(
        Memory<byte> memory,
        Reliability reliability = Reliability.Unreliable,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task DisconnectAsync()
    {
        if (state is RakConnectionState.Handshaking or RakConnectionState.Disconnected)
        {
            return;
        }

        await transport.WriteAsync<DisconnectPacket>(new DisconnectPacket(), Reliability.Unreliable);
        await source.CancelAsync();
        client.Disconnect();
        state = RakConnectionState.Disconnected;
    }

    public ValueTask DisposeAsync()
    {
        client.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task StartAsync()
    {
        await HandleHandshakeAsync();

        while (!source.IsCancellationRequested)
        {
            try
            {
                var messages = await transport.ReadAsync();

                foreach (var message in messages)
                {
                    switch (message.Identifier)
                    {
                        case 0x00:
                            var ping = message.As<ConnectedPingPacket>();

                            await transport.WriteAsync<ConnectedPongPacket>(
                                new ConnectedPongPacket
                                {
                                    Ping = ping.Time,
                                    Pong = ping.Time
                                },
                                Reliability.Unreliable);

                            break;

                        case 0x10:
                            _ = message.As<ConnectionRequestAcceptedPacket>();

                            await transport.WriteAsync<NewIncomingConnectionPacket>(
                                new NewIncomingConnectionPacket
                                {
                                    Server = RemoteEndPoint
                                },
                                Reliability.Unreliable);

                            state = RakConnectionState.Connected;
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        await DisposeAsync();
    }

    private async Task HandleHandshakeAsync()
    {
        await client.WriteAsync(new OpenConnectionRequestFirstPacket
            {
                ProtocolVersion = RakSharp.ProtocolVersion,
                MaximumTransmissionUnit = MaximumTransmissionUnit
            },
            source.Token);

        var message = await client.ReadAsync(source.Token);
        var replyFirst = message.As<OpenConnectionReplyFirstPacket>();

        MaximumTransmissionUnit = replyFirst.MaximumTransmissionUnit;

        await client.WriteAsync(new OpenConnectionRequestSecondPacket
            {
                Server = RemoteEndPoint,
                MaximumTransmissionUnit = MaximumTransmissionUnit,
                Client = client.Identifier
            },
            source.Token);

        message = await client.ReadAsync(source.Token);
        var replySecond = message.As<OpenConnectionReplySecondPacket>();
        MaximumTransmissionUnit = replySecond.MaximumTransmissionUnit;

        await transport.WriteAsync<ConnectionRequestPacket>(
            new ConnectionRequestPacket
            {
                Client = client.Identifier,
                Time = DateTime.UtcNow.Millisecond,
                UseSecurity = false
            },
            Reliability.Unreliable);
    }
}

/// <summary>
/// Stores options that control the <see cref="RakConnection"/>.
/// </summary>
public sealed class RakConnectionOptions
{
    /// <summary>
    /// The <see cref="IPEndPoint"/> to connect to.
    /// </summary>
    public required IPEndPoint RemoteEndPoint { get; init; }

    /// <summary>
    /// Specifies for how long the <see cref="RakConnection"/> should attempt to connect before it times out.
    /// </summary>
    public required TimeSpan TimeOut { get; init; }

    /// <summary>
    /// The maximum amount of bytes that can be transmitted across the network.
    /// </summary>
    public required short MaximumTransmissionUnit { get; init; }
}