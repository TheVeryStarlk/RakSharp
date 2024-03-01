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

internal sealed record Message(int Identifier, ReadOnlyMemory<byte> Memory)
{
    public T As<T>() where T : IIngoingPacket<T>
    {
        if (T.Identifier != Identifier)
        {
            throw new ArgumentException($"Expected {T.Identifier} but got {Identifier} instead.");
        }

        var reader = new MemoryReader(Memory);
        return T.Read(reader);
    }
}