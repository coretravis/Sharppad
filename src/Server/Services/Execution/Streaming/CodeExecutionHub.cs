using Microsoft.AspNetCore.SignalR;

namespace SharpPad.Server.Services.Execution.Streaming;

/// <summary>
/// A simple SignalR hub to handle streaming output and input for code execution sessions.
/// Clients join a group identified by sessionId and can send input back to the server.
/// </summary>
public class CodeExecutionHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Expect a sessionId query parameter to join the corresponding SignalR group.
        var sessionId = Context.GetHttpContext()?.Request.Query["sessionId"].ToString();
        if (!string.IsNullOrEmpty(sessionId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called by the client to provide input. 
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="input">The input string from the client.</param>
    public async Task SendInput(string sessionId, string input)
    {
        // Try to locate the StreamingTextReader for this session.
        if (StreamingTextReaderRegistry.TryGetReader(sessionId, out var reader))
        {
            // Provide the input to unblock Console.ReadLine.
            reader?.ProvideInput(input);
        }
        else
        {
            // No reader found, inform the caller (or handle as needed).
            await Clients.Caller.SendAsync("ReceiveOutput", new
            {
                type = "error",
                content = "No active input stream found for this session."
            });
        }
    }
}