namespace SharpPad.Client.Services.Storage;

/// <summary>
/// Interface for simple storage operations.
/// </summary>
public interface ISimpleStorage
{
    /// <summary>
    /// Retrieves a value from storage asynchronously.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <returns>A task that yields the retrieved value.</returns>
    Task<string> GetAsync(string key);

    /// <summary>
    /// Retrieves a value from storage synchronously.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <returns>The retrieved value.</returns>
    string Get(string key);

    /// <summary>
    /// Sets a value in storage asynchronously.
    /// </summary>
    /// <param name="key">The key to set the value for.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>A task that represents the operation.</returns>
    Task SetAsync(string key, string value);

    /// <summary>
    /// Sets a value in storage synchronously.
    /// </summary>
    /// <param name="key">The key to set the value for.</param>
    /// <param name="value">The value to set.</param>
    void Set(string key, string value);

    /// <summary>
    /// Removes a value from storage asynchronously.
    /// </summary>
    /// <param name="key">The key to remove the value for.</param>
    /// <returns>A task that represents the operation.</returns>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes a value from storage synchronously.
    /// </summary>
    /// <param name="key">The key to remove the value for.</param>
    void Remove(string key);
}