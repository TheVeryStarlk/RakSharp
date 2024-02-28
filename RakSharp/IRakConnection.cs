using System.Net;
using RakSharp.Networking.Packets.Online.FrameSet;

namespace RakSharp;

/// <summary>
/// Represents a RakNet connection to an <see cref="IPEndPoint"/>.
/// </summary>
public interface IRakConnection : IAsyncDisposable
{
    /// <summary>
    /// The <see cref="IPEndPoint"/> which the connection is connected to.
    /// </summary>
    public IPEndPoint RemoteEndPoint { get; }

    /// <summary>
    /// The maximum amount of bytes that can be transmitted across the network.
    /// </summary>
    public short MaximumTransmissionUnit { get; }

    /// <summary>
    /// Writes data to the connected <see cref="IPEndPoint"/>.
    /// </summary>
    /// <param name="memory">The data to send.</param>
    /// <param name="reliability">The <see cref="Reliability"/> type to send the data with.</param>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous write operation.</returns>
    public Task WriteAsync(
        Memory<byte> memory,
        Reliability reliability = Reliability.Unreliable,
        CancellationToken token = default);

    /// <summary>
    /// Reads data from the connected <see cref="IPEndPoint"/>.
    /// </summary>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous read operation,
    /// which wraps the <see cref="Memory{T}"/> containing the contents of data.
    /// </returns>
    public Task<Memory<byte>> ReadAsync(CancellationToken token = default);

    /// <summary>
    /// Disconnects the connection and stops all operations.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous disconnect operation.</returns>
    public Task DisconnectAsync();
}