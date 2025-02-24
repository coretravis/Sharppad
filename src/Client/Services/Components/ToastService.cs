namespace SharpPad.Client.Services.Components;

/// <summary>
/// A service for displaying toast notifications.
/// </summary>
public class ToastService
{
    public event Action<ToastMessage>? OnToastAdded;
    public event Action<Guid>? OnToastRemoved;

    /// <summary>
    /// Displays a toast notification.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="type">The type of the toast notification.</param>
    /// <param name="duration">The duration of the toast notification in milliseconds.</param>
    /// <returns></returns>
    public void ShowToast(string message, ToastType type = ToastType.Info, int duration = 5000)
    {
        var toast = new ToastMessage
        {
            Id = Guid.NewGuid(),
            Message = message,
            Type = type,
            Duration = duration
        };
        OnToastAdded?.Invoke(toast);
    }

    /// <summary>
    /// Removes a toast notification.
    /// </summary>
    /// <param name="id">The ID of the toast notification to remove.</param>
    public void RemoveToast(Guid id)
    {
        OnToastRemoved?.Invoke(id);
    }
}

/// <summary>
/// Represents a toast notification message.
/// </summary>
public class ToastMessage
{
    /// <summary>
    /// Gets or sets the ID of the toast message.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the message content of the toast message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the toast message.
    /// </summary>
    public ToastType Type { get; set; } = ToastType.Info;

    /// <summary>
    /// Gets or sets the duration of the toast message in milliseconds.
    /// </summary>
    public int Duration { get; set; }
}

/// <summary>
/// Represents the type of a toast message.
/// </summary>
public enum ToastType
{
    /// <summary>
    /// Represents a success toast message.
    /// </summary>
    Success,

    /// <summary>
    /// Represents an error toast message.
    /// </summary>
    Error,

    /// <summary>
    /// Represents a warning toast message.
    /// </summary>
    Warning,

    /// <summary>
    /// Represents an information toast message.
    /// </summary>
    Info
}
