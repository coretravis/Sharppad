using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Client.Services.Streaming;
public class CodeExecutionClientService : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<CodeExecutionClientService> _logger;

    public event Action<ExecutionOutput>? OnOutputReceived;
    public event Action<CodeExecutionResponse>? OnExecutionCompleted;

    public string SessionId { get; set; } = string.Empty;

    public CodeExecutionClientService(
        NavigationManager navigationManager,
        IConfiguration configuration,
        ILogger<CodeExecutionClientService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Read hub URL from configuration and ensure it's valid.
        string hubUrl = configuration["SignalR:HubUrl"] ?? $"{navigationManager.BaseUri}codeExecutionHub";
        if (string.IsNullOrWhiteSpace(hubUrl))
        {
            throw new ArgumentException("Hub URL must be provided in configuration.", nameof(configuration));
        }

        // Generate a new session ID.
        SessionId = Guid.NewGuid().ToString();

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{hubUrl}?sessionId={SessionId}")
            .WithAutomaticReconnect()
            .Build();

        // Register lifecycle event handlers to log connection state changes.
        _hubConnection.Closed += async (exception) =>
        {
            _logger.LogWarning(exception, "SignalR connection closed unexpectedly.");
            // No need to reconnect, user starts a connection anyway
            await Task.CompletedTask;
        };

        _hubConnection.Reconnecting += (exception) =>
        {
            _logger.LogWarning(exception, "SignalR connection reconnecting...");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            _logger.LogInformation("SignalR connection reconnected. New connection ID: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        // Subscribe to messages with individual try-catch blocks to prevent one subscriber's error from affecting others.
        _hubConnection.On<ExecutionOutput>("ReceiveOutput", (message) =>
        {
            try
            {
                Console.WriteLine("message: " + message);
                OnOutputReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling 'ReceiveOutput' message.");
            }
        });

        _hubConnection.On<CodeExecutionResponse>("ExecutionComplete", (message) =>
        {
            try
            {
                OnExecutionCompleted?.Invoke(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling 'ExecutionComplete' message.");
            }
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubConnection.StartAsync(cancellationToken);
            _logger.LogInformation("SignalR connection started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting SignalR connection.");
            throw; 
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubConnection.StopAsync(cancellationToken);
            _logger.LogInformation("SignalR connection stopped successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping SignalR connection.");
            throw;
        }
    }

    public async Task SendInputAsync(string input, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubConnection.SendAsync("SendInput", SessionId, input, cancellationToken);
            _logger.LogInformation("Input sent successfully to SignalR hub.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending input to SignalR hub.");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _hubConnection.DisposeAsync();
            _logger.LogInformation("SignalR connection disposed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing SignalR connection.");
        }
    }
}
