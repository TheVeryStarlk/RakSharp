using System.Net;

namespace RakSharp;

/// <summary>
/// Stores options that control the <see cref="RakConnection"/>.
/// </summary>
public sealed class RakConnectionOptions
{
    /// <summary>
    /// The <see cref="IPEndPoint"/> to connect to.
    /// </summary>
    public required IPEndPoint RemoteEndPoint { get; set; }

    /// <summary>
    /// Specifies for how long the <see cref="RakConnection"/> should attempt to connect before it times out.
    /// </summary>
    public required TimeSpan TimeOut { get; set; }

    /// <summary>
    /// The maximum amount of bytes that can be transmitted across the network.
    /// </summary>
    public required short MaximumTransmissionUnit { get; set; }
}

public sealed class RakConnection : IRakConnection
{
    public IPEndPoint RemoteEndPoint => throw new NotImplementedException();

    public short MaximumTransmissionUnit => throw new NotImplementedException();

    /// <summary>
    /// Attempts to initiate a <see cref="RakConnection"/>.
    /// </summary>
    /// <param name="options">The provided <see cref="RakConnectionOptions"/> to control the <see cref="RakConnection"/>.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous connect operation,
    /// which wraps the initiated <see cref="RakConnection"/>.
    /// </returns>
    /// <exception cref="NotImplementedException">The method is not implemented yet.</exception>
    public static Task<IRakConnection> ConnectAsync(RakConnectionOptions options)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(Memory<byte> memory, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<Memory<byte>> ReadAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task DisconnectAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}