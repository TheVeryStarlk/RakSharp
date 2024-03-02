using System.Net;
using RakSharp;
using RakSharp.Networking.Connection;

// var status = await RakSession.PingAsync(new RakSessionOptions
// {
//     RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 19132),
//     TimeOut = TimeSpan.FromSeconds(5)
// });
//
// Console.WriteLine(status.Message);

var source = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    source.Cancel();
};

await using var connection = await RakConnection.ConnectAsync(new RakConnectionOptions
{
    RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 19132),
    TimeOut = TimeSpan.FromSeconds(5),
    MaximumTransmissionUnit = RakNet.MaximumTransmissionUnit
});

var memory = await connection.ReadAsync(source.Token);
Console.WriteLine(string.Join(", ", memory.ToArray()));
await connection.DisconnectAsync();