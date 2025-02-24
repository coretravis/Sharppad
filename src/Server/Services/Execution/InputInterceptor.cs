using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Server.Services.Execution;

/// <summary>
/// Represents an input interceptor that throws an exception when attempting to read from console.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InputInterceptor"/> class.
/// </remarks>
/// <param name="outputs">The list of execution outputs.</param>
public class InputInterceptor(List<ExecutionOutput> outputs) : TextReader
{
    private readonly List<ExecutionOutput> outputs = outputs;

    /// <inheritdoc/>
    public override int Read()
    {
        AddInteractiveModeError();
        throw new InteractiveModeRequiredException();
    }

    /// <inheritdoc/>
    public override string? ReadLine()
    {
        AddInteractiveModeError();
        throw new InteractiveModeRequiredException();
    }

    private void AddInteractiveModeError()
    {
        outputs.Add(new ExecutionOutput
        {
            Content = "Interactive mode is required to read from console",
            Type = OutputType.Error
        });
    }
}
