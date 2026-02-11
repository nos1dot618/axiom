using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axiom.Lsp;

public sealed class LspClient : IDisposable
{
    public bool IsRunning => _process is { HasExited: false };

    private Process? _process;
    private Stream? _stdin;
    private Stream? _stdout;
    private int _id;

    private readonly Dictionary<int, TaskCompletionSource<JsonElement>> _pending = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public Task StartAsync(string serverCommand, string arguments)
    {
        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = serverCommand,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        _process.Start();
        _stdin = _process.StandardInput.BaseStream;
        _stdout = _process.StandardOutput.BaseStream;
        _ = Task.Run(ListenLoopAsync, _cancellationTokenSource.Token);
        _ = Task.Run(ReadStderrAsync);

        return Task.CompletedTask;
    }

    public async Task InitializeAsync(string? rootUri = null)
    {
        var response = await SendRequestAsync("initialize", new
        {
            processId = Environment.ProcessId,
            rootUri,
            capabilities = new { }
        });

        await SendNotificationAsync("initialized", new { });

        Console.WriteLine("LSP initialized:");
        Console.WriteLine(response);
    }

    public async Task ShutdownAsync()
    {
        if (!IsRunning) return;

        try
        {
            await SendRequestAsync("shutdown", null);
            await SendNotificationAsync("exit", null);
        }
        catch
        {
            // ignored
        }

        await _cancellationTokenSource.CancelAsync();

        try
        {
            if (!_process!.WaitForExit(2000)) _process.Kill(true);
        }
        catch
        {
            try
            {
                _process?.Kill();
            }
            catch
            {
                // ignored
            }
        }
    }

    public async Task<JsonElement> SendRequestAsync(string method, object? @params)
    {
        var id = Interlocked.Increment(ref _id);
        var taskCompletionSource =
            new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = taskCompletionSource;

        var payload = new
        {
            jsonrpc = "2.0",
            id,
            method,
            @params
        };

        await SendAsync(payload);
        return await taskCompletionSource.Task;
    }

    public Task SendNotificationAsync(string method, object? @params)
    {
        var payload = new
        {
            jsonrpc = "2.0",
            method,
            @params
        };

        return SendAsync(payload);
    }

    private async Task SendAsync(object payload)
    {
        if (_stdin == null) throw new InvalidOperationException("LSP not started");

        var jsonString = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
        var bytes = Encoding.UTF8.GetBytes(jsonString);
        var header = $"Content-Length: {bytes.Length}\r\n\r\n";
        var headerBytes = Encoding.ASCII.GetBytes(header);

        await _stdin.WriteAsync(headerBytes);
        await _stdin.WriteAsync(bytes);
        await _stdin.FlushAsync();
    }

    private async Task ListenLoopAsync()
    {
        var streamReader = new StreamReader(_stdout!, Encoding.UTF8);

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            var document = await ReadMessageAsync(streamReader);
            if (document == null) continue;
            HandleMessage(document.Value);
        }
    }

    private async Task<JsonElement?> ReadMessageAsync(StreamReader streamReader)
    {
        string? line;
        var contentLength = 0;

        while (!string.IsNullOrEmpty(line = await streamReader.ReadLineAsync()))
        {
            if (!line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) continue;
            var value = line["Content-Length:".Length..].Trim();
            contentLength = int.Parse(value);
        }

        if (contentLength == 0) return null;

        var buffer = new char[contentLength];
        var totalRead = 0;

        while (totalRead < contentLength)
        {
            var read = await streamReader.ReadAsync(buffer, totalRead, contentLength - totalRead);
            if (read == 0) break;
            totalRead += read;
        }

        var message = new string(buffer, 0, totalRead);
        return JsonSerializer.Deserialize<JsonElement>(message);
    }

    private void HandleMessage(JsonElement message)
    {
        // Handle request.
        if (message.TryGetProperty("id", out var idProperty) &&
            idProperty.ValueKind == JsonValueKind.Number)
        {
            var id = idProperty.GetInt32();

            // ReSharper disable once InvertIf
            if (_pending.Remove(id, out var taskCompletionSource))
            {
                if (message.TryGetProperty("result", out var result))
                    taskCompletionSource.SetResult(result);
                else if (message.TryGetProperty("error", out var error))
                    taskCompletionSource.SetException(new Exception(error.GetString()));
            }

            return;
        }

        // Handle notification.
        // ReSharper disable once InvertIf
        if (message.TryGetProperty("method", out var methodProperty) &&
            methodProperty.ValueKind == JsonValueKind.String)
        {
            var method = methodProperty.GetString();
            Console.WriteLine($"LSP Notification: {method}");
        }
    }

    private async Task ReadStderrAsync()
    {
        if (_process == null) return;

        while (!_process.StandardError.EndOfStream)
        {
            var line = await _process.StandardError.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine($"LSP Error: {line}");
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _process?.Dispose();
        _stdin?.Dispose();
        _stdout?.Dispose();
    }
}