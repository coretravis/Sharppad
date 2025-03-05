using Microsoft.AspNetCore.SignalR;
using SharpPad.Shared.Models.Compiler;
using System.Text;

namespace SharpPad.Server.Services.Execution.Streaming;

public class StreamingErrorTextWriter(IHubContext<CodeExecutionHub> hubContext, string sessionId, bool immediateFlush = false) : TextWriter
{
    private readonly IHubContext<CodeExecutionHub> _hubContext = hubContext;
    private readonly string _sessionId = sessionId;
    private readonly StringBuilder _buffer = new StringBuilder();
    private readonly bool _immediateFlush = immediateFlush;

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        _buffer.Append(value);
        if (value == '\n' || _immediateFlush)
        {
            Flush();
        }
    }

    public override void Write(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _buffer.Append(value);
            if (value.Contains("\n") || _immediateFlush)
            {
                Flush();
            }
        }
    }

    public override void Flush()
    {
        if (_buffer.Length > 0)
        {
            var content = _buffer.ToString();
            _buffer.Clear();
            NotifyClient(content);
        }
    }

    private void NotifyClient(string content)
    {
        var output = new ExecutionOutput
        {
            Content = content.TrimEnd(),
            Timestamp = DateTime.UtcNow,
            Type = OutputType.CompilationError,
            Metadata = new Dictionary<string, string>()
        };

        _hubContext.Clients.Group(_sessionId)
            .SendAsync("ReceiveOutput", output);
    }
}
