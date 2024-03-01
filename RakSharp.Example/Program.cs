using System.Net;
using RakSharp.Networking.Connection;

// var status = await RakSession.PingAsync(new RakSessionOptions
// {
//     RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 19132),
//     TimeOut = TimeSpan.FromSeconds(5)
// });
//
// Console.WriteLine(status.Message);

await using var connection = await RakConnection.ConnectAsync(new RakConnectionOptions
{
    RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 19132),
    TimeOut = TimeSpan.FromSeconds(5),
    MaximumTransmissionUnit = 1492
});

Console.ReadLine();
await connection.DisconnectAsync();