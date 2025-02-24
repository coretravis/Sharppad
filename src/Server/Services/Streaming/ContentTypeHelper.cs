using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Server.Services.Streaming;

/// <summary>
/// Helper class for determining the content type of a given string.
/// </summary>
public static class ContentTypeHelper
{
    /// <summary>
    /// Gets the MIME type of the content based on its format.
    /// </summary>
    /// <param name="content">The content to determine the MIME type for.</param>
    /// <returns>The MIME type of the content.</returns>
    public static string GetContentMimeType(string content)
    {
        if (IsJson(content)) return "application/json";
        if (IsXml(content)) return "application/xml";
        if (IsHtml(content)) return "text/html";
        return "text/plain";
    }

    /// <summary>
    /// Determines if the content is in JSON format.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns>True if the content is in JSON format; otherwise, false.</returns>
    public static bool IsJson(string content)
    {
        content = content.TrimStart();
        return (content.StartsWith("{") && content.EndsWith("}")) ||
               (content.StartsWith("[") && content.EndsWith("]"));
    }

    /// <summary>
    /// Determines if the content is in XML format.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns>True if the content is in XML format; otherwise, false.</returns>
    public static bool IsXml(string content)
    {
        content = content.TrimStart();
        return content.StartsWith("<?xml") || content.StartsWith("<");
    }

    /// <summary>
    /// Determines if the content is in HTML format.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns>True if the content is in HTML format; otherwise, false.</returns>
    public static bool IsHtml(string content)
    {
        content = content.TrimStart();
        return content.StartsWith("<!DOCTYPE html>") ||
               content.StartsWith("<html>") ||
               content.Contains("<body>");
    }

    /// <summary>
    /// Determines the output type based on the content format.
    /// </summary>
    /// <param name="content">The content to determine the output type for.</param>
    /// <returns>The output type of the content.</returns>
    public static OutputType DetermineOutputType(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return OutputType.Text;

        try
        {
            if (IsJson(content)) return OutputType.Json;
            if (IsXml(content)) return OutputType.Xml;
            if (IsHtml(content)) return OutputType.Html;
            return OutputType.Text;
        }
        catch
        {
            return OutputType.Text;
        }
    }
}
