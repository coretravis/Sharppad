using Microsoft.AspNetCore.SignalR;
using SharpPad.Shared.Models.Compiler;
using System.Text;

namespace SharpPad.Server.Services.Streaming;

/// <summary>
/// A custom TextReader that blocks on Console.ReadLine until input is provided by the client.
/// </summary>
public class StreamingTextReader(IHubContext<CodeExecutionHub> hubContext, string sessionId) : TextReader
{
    private TaskCompletionSource<string> _inputTcs = new TaskCompletionSource<string>();

    private readonly IHubContext<CodeExecutionHub> _hubContext = hubContext;
    private readonly string _sessionId = sessionId;

    /// <summary>
    /// Called by the SignalR hub when client input is received.
    /// </summary>
    /// <param name="input">The input provided by the client.</param>
    public void ProvideInput(string input)
    {
        _inputTcs.TrySetResult(input);
        _inputTcs = new TaskCompletionSource<string>();
    }

    public override string? ReadLine()
    {
        var output = new ExecutionOutput
        {
            Content = "",
            Timestamp = DateTime.UtcNow,
            Type = OutputType.ConsoleInput,
            Metadata = new Dictionary<string, string>()
        };

        _hubContext.Clients.Group(_sessionId)
            .SendAsync("ReceiveOutput", output);

        // Blocks until ProvideInput is called.
        return _inputTcs.Task.GetAwaiter().GetResult();
    }
}
