namespace SharpPad.Client.Services.Caching;

public interface IClientCache
{
    /// <summary>
    /// Sets an item in the cache with the specified key and expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="key">The key of the item.</param>
    /// <param name="item">The item to be stored in the cache.</param>
    /// <param name="expiration">The expiration time for the item.</param>
    void Set<T>(string key, T item, TimeSpan expiration);

    /// <summary>
    /// Tries to get the item from the cache with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="key">The key of the item.</param>
    /// <param name="item">When this method returns, contains the item associated with the specified key, if the key is found; otherwise, the default value for the type of the item parameter.</param>
    /// <returns><c>true</c> if the cache contains an item with the specified key; otherwise, <c>false</c>.</returns>
    bool TryGetValue<T>(string key, out T item);
}
