using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Axiom.Lsp;

public class LspClient
{
    private Process _process;
    private StreamWriter _input;
    private StreamReader _output;
    private int _id = 0;

    public async Task StartAsync(string serverCommand, string arguments)
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
        _input = _process.StandardInput;
        _output = _process.StandardOutput;
        _ = Task.Run(ListenAsync);
    }

    public async Task InitializeAsync()
    {
        var request = new
        {
            jsonrpc = "2.0",
            id = _id++,
            method = "initialize",
            @params = new
            {
                processId = Process.GetCurrentProcess().Id,
                rootUri = (string?)null,
                capabilities = new { }
            }
        };

        await SendAsync(request);
    }

    public async Task ShutdownAsync()
    {
        if (_process.HasExited) return;

        var shutdownRequest = new
        {
            jsonRpc = "2.0",
            id = _id++,
            method = "shutdown",
            @params = (object?)null
        };

        await SendAsync(shutdownRequest);

        var exitNotification = new
        {
            jsonrpc = "2.0",
            method = "exit"
        };

        await SendAsync(exitNotification);

        try
        {
            if (!_process.WaitForExit(2000)) _process.Kill(true);
        }
        catch
        {
            _process.Kill();
        }
    }

    private async Task SendAsync(object request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = Encoding.UTF8.GetBytes(json);

        await _input.WriteAsync($"Content-Length: {content.Length}\r\n\r\n{json}");
        await _input.FlushAsync();
    }

    private async Task ListenAsync()
    {
        while (!_output.EndOfStream)
        {
            var line = await _output.ReadLineAsync();
            if (line == null || !line.StartsWith("Content-Length")) continue;

            var length = int.Parse(line.Split(":")[1].Trim());
            await _output.ReadLineAsync(); // Empty line.
            var buffer = new char[length];
            await _output.ReadAsync(buffer, 0, length);
            var message = new string(buffer);
            Console.WriteLine(message);
        }
    }
}