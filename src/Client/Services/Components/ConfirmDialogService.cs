namespace SharpPad.Client.Services.Components;

/// <summary>
/// Service for displaying confirmation dialogs.
/// </summary>
public class ConfirmDialogService
{
    /// <summary>
    /// Fired when a confirmation dialog should be shown.
    /// </summary>
    public event Action<ConfirmDialogRequest>? OnShow;

    /// <summary>
    /// Requests a confirmation dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="confirmButtonText">Text for the confirm button.</param>
    /// <param name="cancelButtonText">Text for the cancel button.</param>
    /// <returns>A task that resolves to true if confirmed; otherwise, false.</returns>
    public Task<bool> ShowConfirmAsync(
        string title = "Confirm",
        string message = "Are you sure you want to proceed?",
        string confirmButtonText = "Yes",
        string cancelButtonText = "No")
    {
        var tcs = new TaskCompletionSource<bool>();
        var request = new ConfirmDialogRequest
        {
            Title = title,
            Message = message,
            ConfirmButtonText = confirmButtonText,
            CancelButtonText = cancelButtonText,
            TaskCompletionSource = tcs
        };

        OnShow?.Invoke(request);
        return tcs.Task;
    }
}

/// <summary>
/// Contains the parameters for a confirmation dialog request.
/// </summary>
public class ConfirmDialogRequest
{
    public string Title { get; set; } = "Confirm";
    public string Message { get; set; } = "Are you sure you want to proceed?";
    public string ConfirmButtonText { get; set; } = "Yes";
    public string CancelButtonText { get; set; } = "No";
    public TaskCompletionSource<bool> TaskCompletionSource { get; set; } = default!;
}
