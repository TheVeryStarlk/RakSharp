using System.Net;
using RakSharp;

await using var connection = await RakConnection.ConnectAsync(new RakConnectionOptions
{
    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 19132),
    TimeOut = TimeSpan.FromSeconds(5),
    MaximumTransmissionUnit = 1492
});