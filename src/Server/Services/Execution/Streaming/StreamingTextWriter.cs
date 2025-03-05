using Microsoft.AspNetCore.SignalR;
using SharpPad.Shared.Models.Compiler;
using System.Text;

namespace SharpPad.Server.Services.Execution.Streaming;

public class StreamingTextWriter(IHubContext<CodeExecutionHub> hubContext, string sessionId, bool immediateFlush = false) : TextWriter
{
    private readonly IHubContext<CodeExecutionHub> _hubContext = hubContext;
    private readonly string _sessionId = sessionId;
    private char _buffer = '0';
    private readonly bool _immediateFlush = immediateFlush;

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        _buffer = value;
        Flush();
    }

    public override void Flush()
    {
        NotifyClient(_buffer);
    }

    private void NotifyClient(char content)
    {
        var output = new ExecutionOutput
        {
            CharContent = content,
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>()
        };

        _hubContext.Clients.Group(_sessionId)
            .SendAsync("ReceiveOutput", output);
    }
}
