namespace RakSharp.Networking.Packets.Online.FrameSet;

/// <summary>
/// Stores the different reliability types.
/// </summary>
/// <remarks>
/// The other reliability types are not used by Minecraft: Bedrock edition,
/// which are not present in the <see cref="Enum"/>.
/// </remarks>
public enum Reliability
{
    /// <summary>
    /// Represents an unreliable type for data,
    /// this does not guarantee data to have any kind of reliability or order.
    /// </summary>
    Unreliable = 0x00,

    /// <summary>
    /// Represents a reliable type for data,
    /// this guarantees that data have some kind of reliability.
    /// </summary>
    Reliable = 0x02,

    /// <summary>
    /// Represents a reliable type for data,
    /// this guarantees that data have some kind of reliability and order.
    /// </summary>
    ReliableOrdered = 0x03
}