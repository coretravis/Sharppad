using System.Text;
using System.Text.Json;
using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Server.Services.Execution;

/// <summary>
/// Intercepts console output and stores it in a list of ExecutionOutput objects.
/// </summary>
public class OutputInterceptor(List<ExecutionOutput> outputs, int maxOutputSize = 1024 * 1024) : TextWriter
{
    private readonly List<ExecutionOutput> outputs = outputs;
    private readonly StringBuilder currentBuffer = new();
    private readonly int maxOutputSize = maxOutputSize;
    private int totalOutputSize = 0;
    private bool outputLimitReached = false;

    public override Encoding Encoding => Encoding.UTF8;

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    /// <param name="value"></param>
    public override void Write(char value)
    {
        if (outputLimitReached) return;

        if (totalOutputSize + 1 > maxOutputSize)
        {
            OutputLimitReached();
            return;
        }

        currentBuffer.Append(value);
        totalOutputSize++;

        if (value == '\n')
        {
            FlushBuffer();
        }
    }

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    /// <param name="value"></param>
    public override void Write(string? value)
    {
        if (outputLimitReached || string.IsNullOrEmpty(value)) return;

        if (totalOutputSize + value.Length > maxOutputSize)
        {
            var remainingSpace = maxOutputSize - totalOutputSize;
            if (remainingSpace > 0)
            {
                currentBuffer.Append(value.Substring(0, remainingSpace));
                totalOutputSize += remainingSpace;
            }
            OutputLimitReached();
            return;
        }

        currentBuffer.Append(value);
        totalOutputSize += value.Length;
    }


    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    private void OutputLimitReached()
    {
        outputLimitReached = true;
        FlushBuffer();
        AddOutput($"\nOutput limit of {maxOutputSize / 1024}KB reached. Output truncated.");
    }

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    /// <param name="value"></param>
    public override void WriteLine(string? value)
    {

        Write(value);
        Write('\n');
    }

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    private void FlushBuffer()
    {
        var content = currentBuffer.ToString();
        if (!string.IsNullOrEmpty(content))
        {
            AddOutput(content);
            currentBuffer.Clear();
        }
    }

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    public override void Flush()
    {
        FlushBuffer();
    }

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    /// <param name="content"></param>
    private void AddOutput(string content)
    {
        var output = new ExecutionOutput
        {
            Content = content.TrimEnd(),
            Timestamp = DateTime.UtcNow,
            Type = DetermineOutputType(content),
            Metadata = new Dictionary<string, string>
            {
                { "Size", content.Length.ToString() },
                { "ContentType", GetContentMimeType(content) }
            }
        };

        if (output.Type == OutputType.Json)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);
                output.Content = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    MaxDepth = 64
                });
            }
            catch (Exception ex)
            {
                output.Metadata["JsonError"] = ex.Message;
            }
        }

        outputs.Add(output);
    }

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    /// <param name="content"></param>
    private string GetContentMimeType(string content)
    {
        if (IsJson(content)) return "application/json";
        if (IsXml(content)) return "application/xml";
        if (IsHtml(content)) return "text/html";
        return "text/plain";
    }

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    /// <param name="content"></param>
    private bool IsJson(string content)
    {
        content = content.TrimStart();
        return (content.StartsWith("{") && content.EndsWith("}")) ||
               (content.StartsWith("[") && content.EndsWith("]"));
    }

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    /// <param name="content"></param>
    private bool IsXml(string content)
    {
        content = content.TrimStart();
        return content.StartsWith("<?xml") || content.StartsWith("<");
    }

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    /// <param name="content"></param>
    private bool IsHtml(string content)
    {
        content = content.TrimStart();
        return content.StartsWith("<!DOCTYPE html>") ||
               content.StartsWith("<html>") ||
               content.Contains("<body>");
    }

    /// <summary>
    /// Intercepts console output and stores it in a list of ExecutionOutput objects.
    /// </summary>
    /// <param name="content"></param>
    private OutputType DetermineOutputType(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return OutputType.Text;

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                return OutputType.Collection;
            return OutputType.Json;
        }
        catch
        {
            if (IsXml(content)) return OutputType.Xml;
            if (IsHtml(content)) return OutputType.Html;
            return OutputType.Text;
        }
    }
}
