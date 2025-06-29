using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Component.Cache.Models;

namespace Component.Cache.Contract
{
   /// <summary>
   /// Defines the contract for cache backend implementations that handle low-level cache operations.
   /// </summary>
   /// <typeparam name="TBuffer">The type of buffer used to store serialized data in the cache.</typeparam>
   public interface ICacheBackend<TBuffer>
   {
      /// <summary>
      /// Gets the type of cache backend.
      /// </summary>
      CacheType CacheType { get; }

      /// <summary>
      /// Retrieves a cached value by its key.
      /// </summary>
      /// <param name="key">The unique identifier for the cached item.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>The cached buffer, or null if the key is not found.</returns>
      Task<TBuffer?> GetAsync(string key, CancellationToken cancellationToken = default);

      /// <summary>
      /// Stores a value in the cache with the specified key, expiration, and tags.
      /// </summary>
      /// <param name="key">The unique identifier for the cached item.</param>
      /// <param name="buffer">The serialized data to store in the cache.</param>
      /// <param name="expiration">The expiration configuration for the cached item.</param>
      /// <param name="tags">Tags associated with the cached item for group operations.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>Task of execution.</returns>
      Task SetAsync(
          string key,
          TBuffer buffer,
          CacheExpirationOptions expiration,
          string[] tags,
          CancellationToken cancellationToken = default);

      /// <summary>
      /// Removes a cached item by its key.
      /// </summary>
      /// <param name="key">The unique identifier for the cached item to remove.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>Task of execution.</returns>
      Task RemoveAsync(string key, CancellationToken cancellationToken = default);

      /// <summary>
      /// Removes all cached items whose keys start with the specified prefix.
      /// </summary>
      /// <param name="prefix">The prefix to match against cache keys.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>Task of execution.</returns>
      Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

      /// <summary>
      /// Removes all cached items that are associated with any of the specified tags.
      /// </summary>
      /// <param name="tags">The tags to match for removal.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>Task of execution.</returns>
      Task RemoveByTagsAsync(string[] tags, CancellationToken cancellationToken = default);

      /// <summary>
      /// Removes all items from the cache.
      /// </summary>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>Task of execution.</returns>
      Task ClearAsync(CancellationToken cancellationToken = default);

      /// <summary>
      /// Determines whether a cached item exists for the specified key.
      /// </summary>
      /// <param name="key">The unique identifier to check for existence.</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>True if the key exists in the cache; otherwise, false.</returns>
      Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

      /// <summary>
      /// Retrieves all cache keys, optionally filtered by a pattern.
      /// </summary>
      /// <param name="pattern">Optional pattern to filter keys (e.g., "user:*").</param>
      /// <param name="cancellationToken">Token to cancel the operation.</param>
      /// <returns>A collection of cache keys matching the pattern.</returns>
      Task<IEnumerable<string>> GetKeysAsync(string? pattern = null, CancellationToken cancellationToken = default);
   }
}
