using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Axiom.Common;
using Axiom.Diagnostics;
using Axiom.Lsp.Language;

namespace Axiom.Lsp.Transport;

public sealed class JsonRpcLspClient : IAsyncDisposable
{
    private Process? _process;
    private Stream? _stdin;
    private Stream? _stdout;

    private int _requestId;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> _pendingRequests = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Logger _logger = new(ModuleType.Lsp, LogLevel.Debug);
    private bool IsRunning => _process is { HasExited: false };

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public LspServerConfiguration Configuration { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public JsonRpcLspClient(LspServerConfiguration configuration)
    {
        Configuration = configuration;
    }

    public Task StartAsync()
    {
        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Configuration.Command,
                Arguments = Configuration.Arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _process.Start();
        _stdin = _process.StandardInput.BaseStream;
        _stdout = _process.StandardOutput.BaseStream;
        _ = Task.Run(ListenLoopAsync, _cancellationTokenSource.Token);
        _ = Task.Run(ReadStderrAsync);

        return Task.CompletedTask;
    }

    public async Task<JsonElement> SendRequestAsync(string method, object? @params)
    {
        var requestId = Interlocked.Increment(ref _requestId);
        var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests[requestId] = tcs;

        await SendAsync(new
        {
            jsonrpc = "2.0",
            id = requestId,
            method,
            @params
        });

        return await tcs.Task;
    }

    public Task SendNotificationAsync(string method, object? @params = null)
    {
        if (@params == null)
        {
            return SendAsync(new
            {
                jsonrpc = "2.0",
                method
            });
        }

        return SendAsync(new
        {
            jsonrpc = "2.0",
            method,
            @params
        });
    }

    private async Task SendAsync(object payload)
    {
        var jsonString = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
        var bytes = Encoding.UTF8.GetBytes(jsonString);
        var header = $"Content-Length: {bytes.Length}\r\n\r\n";
        var headerBytes = Encoding.ASCII.GetBytes(header);

        _logger.Debug($"Client: {jsonString}");

        await _stdin!.WriteAsync(headerBytes);
        await _stdin.WriteAsync(bytes);
        await _stdin.FlushAsync();
    }

    private async Task ListenLoopAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            var document = await ReadMessageAsync(_stdout!);
            if (document == null) continue;

            HandleMessage(document.Value);
        }
    }


    private static async Task<JsonElement?> ReadMessageAsync(Stream stream)
    {
        var headerBuffer = new List<byte>();
        var temp = new byte[1];

        while (true)
        {
            var read = await stream.ReadAsync(temp.AsMemory(0, 1));
            if (read == 0) return null;

            headerBuffer.Add(temp[0]);

            // Check for \r\n\r\n
            if (headerBuffer.Count >= 4 &&
                headerBuffer[^4] == '\r' &&
                headerBuffer[^3] == '\n' &&
                headerBuffer[^2] == '\r' &&
                headerBuffer[^1] == '\n') break;
        }

        var headerText = Encoding.ASCII.GetString(headerBuffer.ToArray());

        var contentLengthLine = headerText
            .Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(h => h.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));

        if (contentLengthLine == null) return null;

        var contentLength = int.Parse(contentLengthLine["Content-Length:".Length..].Trim());
        var bodyBuffer = new byte[contentLength];
        var totalRead = 0;

        while (totalRead < contentLength)
        {
            var read = await stream.ReadAsync(bodyBuffer.AsMemory(totalRead, contentLength - totalRead));
            if (read == 0) throw new EndOfStreamException("Unexpected end of LSP stream.");

            totalRead += read;
        }

        var json = Encoding.UTF8.GetString(bodyBuffer);
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    private void HandleMessage(JsonElement message)
    {
        // Handle request.
        if (message.TryGetProperty("id", out var idProperty) && idProperty.ValueKind == JsonValueKind.Number)
        {
            var id = idProperty.GetInt32();

            if (!_pendingRequests.TryRemove(id, out var taskCompletionSource)) return;

            if (message.TryGetProperty("result", out var result)) taskCompletionSource.SetResult(result);
            else if (message.TryGetProperty("error", out var error))
                taskCompletionSource.SetException(new Exception(error.GetString()));

            _logger.Debug($"Server: message={message}");
            return;
        }

        // Handle notification.
        if (!message.TryGetProperty("method", out _)) return;
        _logger.Debug($"Server: message={message}");
    }

    private async Task ReadStderrAsync()
    {
        if (_process == null) return;

        while (!_process.StandardError.EndOfStream)
        {
            var line = await _process.StandardError.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
            {
                _logger.Error($"Server: error={line}");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.CancelAsync();

        if (IsRunning)
        {
            await SendNotificationAsync(LspMethod.Notification.Shutdown);
            await SendNotificationAsync(LspMethod.Notification.Exit);

            if (_stdin is not null) await _stdin.DisposeAsync();
            if (_stdout is not null) await _stdout.DisposeAsync();

            _process!.Kill();
        }

        _process?.Dispose();
    }
}