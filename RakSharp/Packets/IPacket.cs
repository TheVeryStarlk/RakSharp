namespace RakSharp.Packets;

internal interface IPacket
{
    public static abstract byte Identifier { get; }
}

internal interface IIngoingPacket<out TSelf> : IPacket where TSelf : IIngoingPacket<TSelf>
{
    public static abstract TSelf Read(MemoryReader reader);
}

internal interface IOutgoingPacket : IPacket
{
    public void Write(ref MemoryWriter writer);
}