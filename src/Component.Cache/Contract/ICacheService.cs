using System;
using System.Threading;
using System.Threading.Tasks;
using Component.Cache.Models;

namespace Component.Cache.Contract
{
   /// <summary>
   /// Provides high-level cache operations with automatic serialization and deserialization.
   /// </summary>
   public interface ICacheService
   {
      /// <summary>
      /// Retrieves a cached value and deserializes it to the specified type.
      /// </summary>
      /// <typeparam name="T">The type to deserialize the cached value to.</typeparam>
      /// <param name="key">The unique identifier for the cached item.</param>
      /// <param name="options">Optional configuration for the cache operation.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>The deserialized cached value, or null if not found.</returns>
      Task<T?> GetAsync<T>(string key, CacheOptions? options = null, CancellationToken cancellationToken = default);

      /// <summary>
      /// Retrieves a cached value or executes the factory function to create and cache the value.
      /// Implements the cache-aside pattern for efficient data access.
      /// </summary>
      /// <typeparam name="T">The type of the value to cache and return.</typeparam>
      /// <param name="key">The unique identifier for the cached item.</param>
      /// <param name="factory">The factory function to execute if the key is not found in cache.</param>
      /// <param name="options">Optional configuration for the cache operation.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>The cached value or the result of the factory function.</returns>
      Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default);

      /// <summary>
      /// Serializes and stores a value in the cache with the specified key.
      /// </summary>
      /// <typeparam name="T">The type of the value to cache.</typeparam>
      /// <param name="key">The unique identifier for the cached item.</param>
      /// <param name="value">The value to serialize and store in the cache.</param>
      /// <param name="options">Optional configuration for the cache operation.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>Task of execution.</returns>
      Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default);

      /// <summary>
      /// Removes a cached item by its key.
      /// </summary>
      /// <param name="key">The unique identifier for the cached item to remove.</param>
      /// <param name="options">Optional configuration for the cache operation.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>Task of execution.</returns>
      Task RemoveAsync(string key, CacheOptions? options = null, CancellationToken cancellationToken = default);

      /// <summary>
      /// Removes all cached items whose keys start with the specified prefix.
      /// </summary>
      /// <param name="prefix">The prefix to match against cache keys.</param>
      /// <param name="options">Optional configuration for the cache operation.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>Task of execution.</returns>
      Task RemoveByPrefixAsync(string prefix, CacheOptions? options = null, CancellationToken cancellationToken = default);

      /// <summary>
      /// Removes all cached items that are associated with any of the specified tags.
      /// </summary>
      /// <param name="tags">The tags to match for removal.</param>
      /// <param name="options">Optional configuration for the cache operation.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>Task of execution.</returns>
      Task RemoveByTagsAsync(string[] tags, CacheOptions? options = null, CancellationToken cancellationToken = default);

      /// <summary>
      /// Removes all items from the cache.
      /// </summary>
      /// <param name="options">Optional configuration for the cache operation.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>Task of execution.</returns>
      Task ClearAsync(CacheOptions? options = null, CancellationToken cancellationToken = default);

      /// <summary>
      /// Determines whether a cached item exists for the specified key.
      /// </summary>
      /// <param name="key">The unique identifier to check for existence.</param>
      /// <param name="options">Optional configuration for the cache operation.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>True if the key exists in the cache; otherwise, false.</returns>
      Task<bool> ExistsAsync(string key, CacheOptions? options = null, CancellationToken cancellationToken = default);
   }
}
