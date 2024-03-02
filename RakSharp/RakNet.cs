namespace RakSharp;

/// <summary>
/// Stores useful information used throughout the code-base about the protocol.
/// </summary>
public static class RakNet
{
    /// <summary>
    /// The supported protocol version of RakNet.
    /// </summary>
    public static byte ProtocolVersion => 11;

    /// <summary>
    /// The highest "possible" value for a maximum transmission unit.
    /// </summary>
    public static short MaximumTransmissionUnit => 1472;
}