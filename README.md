# RakSharp
A simple and light RakNet library that was designed to work with Minecraft: Bedrock edition. 
This library makes assumptions about how the protocol works to achieve compatibility with Minecraft.

## Usage
This is currently just a draft.

#### Pinging a server
```cs
var pong = await RakSession.PingAsync(new RakSessionOptions
{
    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 19132),
    TimeOut = TimeSpan.FromSeconds(5)
});
```

#### Conecting to a server
```cs
await using var connection = await RakConnection.ConnectAsync(new RakConnectionOptions
{
    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 19132),
    TimeOut = TimeSpan.FromSeconds(5),
    MaximumTransmissionUnit = 1492
});
```