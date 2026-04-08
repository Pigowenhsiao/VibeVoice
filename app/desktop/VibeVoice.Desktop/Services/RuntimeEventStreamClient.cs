using System.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using VibeVoice.Desktop.Models;

namespace VibeVoice.Desktop.Services;

public interface IRuntimeEventStreamClient
{
    IAsyncEnumerable<RuntimeJobEvent> StreamJobEventsAsync(string jobId, CancellationToken cancellationToken = default);
}

public sealed class RuntimeEventStreamClient : IRuntimeEventStreamClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Uri _webSocketBaseAddress;

    public RuntimeEventStreamClient(string baseAddress)
    {
        var baseUri = new Uri(baseAddress, UriKind.Absolute);
        var builder = new UriBuilder(baseUri)
        {
            Scheme = baseUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws",
        };
        _webSocketBaseAddress = builder.Uri;
    }

    public async IAsyncEnumerable<RuntimeJobEvent> StreamJobEventsAsync(
        string jobId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var socket = new ClientWebSocket();
        var endpoint = new Uri(_webSocketBaseAddress, $"/ws/jobs/{jobId}/events");
        await socket.ConnectAsync(endpoint, cancellationToken);

        var buffer = new byte[4096];
        while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
        {
            using var messageBuffer = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    yield break;
                }

                await messageBuffer.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken);
            }
            while (!result.EndOfMessage);

            var messageJson = Encoding.UTF8.GetString(messageBuffer.ToArray());
            var runtimeEvent = JsonSerializer.Deserialize<RuntimeJobEvent>(messageJson, JsonOptions);
            if (runtimeEvent is not null)
            {
                yield return runtimeEvent;
            }
        }
    }
}
