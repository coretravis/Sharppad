namespace SharpPad.Server.Services.Execution;

/// <summary>
/// Exception thrown when interactive mode is required to read from console.
/// </summary>
public class InteractiveModeRequiredException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveModeRequiredException"/> class.
    /// </summary>
    public InteractiveModeRequiredException() : base("Interactive mode is required to read from console")
    {
    }

    public InteractiveModeRequiredException(string message) : base(message)
    {
        
    }
}
