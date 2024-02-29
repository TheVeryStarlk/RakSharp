using System.Net;
using RakSharp.Networking.Session;
using RakSharp.Packets;
using RakSharp.Packets.Online.FrameSet;

namespace RakSharp.Networking.Connection;

/// <inheritdoc />
public sealed class RakConnection : IRakConnection
{
    public IPEndPoint RemoteEndPoint => throw new NotImplementedException();

    public short MaximumTransmissionUnit => throw new NotImplementedException();

    private RakConnectionState state;

    private readonly CancellationTokenSource source = new CancellationTokenSource();
    private readonly RakClient client;

    private RakConnection(RakClient client)
    {
        this.client = client;
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

        var connection = new RakConnection(await RakClient.ConnectAsync(options.RemoteEndPoint));
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
        await source.CancelAsync();

        if (state is RakConnectionState.Handshaking or RakConnectionState.Disconnected)
        {
            return;
        }

        state = RakConnectionState.Disconnected;
    }

    public async ValueTask DisposeAsync()
    {
        await client.DisposeAsync();
    }

    private async Task StartAsync()
    {
        while (!source.IsCancellationRequested)
        {
            try
            {

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
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