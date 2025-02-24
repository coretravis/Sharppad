using System.Collections.Concurrent;

namespace SharpPad.Server.Services.Streaming;

public static class StreamingTextReaderRegistry
{
    // Thread-safe dictionary to store StreamingTextReader instances keyed by sessionId.
    private static readonly ConcurrentDictionary<string, StreamingTextReader> _readers
        = new ConcurrentDictionary<string, StreamingTextReader>();

    /// <summary>
    /// Registers a StreamingTextReader for the given session.
    /// </summary>
    public static void Register(string sessionId, StreamingTextReader reader)
    {
        _readers[sessionId] = reader;
    }

    /// <summary>
    /// Unregisters the StreamingTextReader for the given session.
    /// </summary>
    public static void Unregister(string sessionId)
    {
        _readers.TryRemove(sessionId, out _);
    }

    /// <summary>
    /// Tries to get the StreamingTextReader associated with the given session.
    /// </summary>
    public static bool TryGetReader(string sessionId, out StreamingTextReader? reader)
    {
        return _readers.TryGetValue(sessionId, out reader);
    }
}
