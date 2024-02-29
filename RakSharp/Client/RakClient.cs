using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;

namespace RakSharp.Client;

internal sealed class RakClient : ConnectionContext
{
    public override IDuplexPipe Transport { get; set; }

    public override IFeatureCollection Features { get; } = new FeatureCollection();

    public override string ConnectionId { get; set; } = Guid.NewGuid().ToString();

    public override IDictionary<object, object?> Items { get; set; } = new ConnectionItems();

    private volatile bool aborted;

    private readonly Socket socket;
    private readonly EndPoint endPoint;
    private readonly IDuplexPipe application;

    private RakClient(IPEndPoint endPoint)
    {
        this.endPoint = endPoint;

        socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

        var pair = DuplexPipe.CreateConnectionPair();
        Transport = pair.Transport;
        application = pair.Application;
    }

    public static async Task<RakClient> ConnectAsync(IPEndPoint remoteEndPoint)
    {
        var client = new RakClient(remoteEndPoint);
        await client.StartAsync();
        return client;
    }

    public async ValueTask<ConnectionContext> StartAsync()
    {
        await socket.ConnectAsync(endPoint).ConfigureAwait(false);
        LocalEndPoint = socket.LocalEndPoint;
        RemoteEndPoint = socket.RemoteEndPoint;

        _ = ExecuteAsync();

        return this;
    }

    private async Task ExecuteAsync()
    {
        Exception? sendError = null;

        try
        {
            var receiveTask = DoReceive();
            var sendTask = DoSend();

            if (await Task.WhenAny(receiveTask, sendTask).ConfigureAwait(false) == sendTask)
            {
                // Tell the reader it's being aborted
                socket.Dispose();
            }

            await receiveTask;
            sendError = await sendTask;

            socket.Dispose();
        }
        finally
        {
            await application.Input.CompleteAsync(sendError);
        }
    }

    private async Task DoReceive()
    {
        Exception? error = null;

        try
        {
            await ProcessReceives().ConfigureAwait(false);
        }
        catch (SocketException exception) when (exception.SocketErrorCode is SocketError.ConnectionReset)
        {
            error = new ConnectionResetException(exception.Message, exception);
        }
        catch (SocketException exception) when (exception.SocketErrorCode is SocketError.OperationAborted
                                                    or SocketError.ConnectionAborted
                                                    or SocketError.Interrupted
                                                    or SocketError.InvalidArgument)
        {
            if (!aborted)
            {
                error = new ConnectionAbortedException();
            }
        }
        catch (ObjectDisposedException)
        {
            if (!aborted)
            {
                error = new ConnectionAbortedException();
            }
        }
        catch (IOException exception)
        {
            error = exception;
        }
        catch (Exception exception)
        {
            error = new IOException(exception.Message, exception);
        }
        finally
        {
            if (aborted)
            {
                error ??= new ConnectionAbortedException();
            }

            await application.Output.CompleteAsync(error).ConfigureAwait(false);
        }
    }

    private async Task ProcessReceives()
    {
        while (true)
        {
            var buffer = application.Output.GetMemory();
            var bytesReceived = await socket.ReceiveAsync(buffer);

            application.Output.Advance(bytesReceived);

            var result = await application.Output.FlushAsync();

            if (result.IsCompleted)
            {
                break;
            }
        }
    }

    private async Task<Exception?> DoSend()
    {
        Exception? error = null;

        try
        {
            await ProcessSends().ConfigureAwait(false);
        }
        catch (SocketException exception) when (exception.SocketErrorCode is SocketError.OperationAborted)
        {
            error = null;
        }
        catch (ObjectDisposedException)
        {
            error = null;
        }
        catch (IOException exception)
        {
            error = exception;
        }
        catch (Exception exception)
        {
            error = new IOException(exception.Message, exception);
        }
        finally
        {
            aborted = true;
            socket.Shutdown(SocketShutdown.Both);
        }

        return error;
    }

    private async Task ProcessSends()
    {
        while (true)
        {
            var result = await application.Input.ReadAsync().ConfigureAwait(false);
            var buffer = result.Buffer;

            if (result.IsCanceled)
            {
                break;
            }

            var end = buffer.End;
            var isCompleted = result.IsCompleted;

            if (!buffer.IsEmpty)
            {
                await socket.SendAsync(buffer.ToArray());
            }

            application.Input.AdvanceTo(end);

            if (isCompleted)
            {
                break;
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await Transport.Output.CompleteAsync().ConfigureAwait(false);
        await Transport.Input.CompleteAsync().ConfigureAwait(false);
    }
}

internal sealed class DuplexPipe(PipeReader reader, PipeWriter writer) : IDuplexPipe
{
    public PipeReader Input { get; } = reader;

    public PipeWriter Output { get; } = writer;

    public static (IDuplexPipe Transport, IDuplexPipe Application) CreateConnectionPair()
    {
        var input = new Pipe();
        var output = new Pipe();

        var transportToApplication = new DuplexPipe(output.Reader, input.Writer);
        var applicationToTransport = new DuplexPipe(input.Reader, output.Writer);

        return (applicationToTransport, transportToApplication);
    }
}