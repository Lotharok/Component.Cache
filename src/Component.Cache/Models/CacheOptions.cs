using System;

namespace Component.Cache.Models
{
   /// <summary>
   /// Configuration options for cache operations, including backend selection, serialization, and expiration settings.
   /// </summary>
   public class CacheOptions
   {
      /// <summary>
      /// Gets or sets the type of cache backend to use for this operation.
      /// Defaults to <see cref="CacheType.InMemory"/>.
      /// </summary>
      public CacheType CacheType { get; set; } = CacheType.InMemory;

      /// <summary>
      /// Gets or sets the serialization format to use for this operation.
      /// Defaults to <see cref="SerializerType.Json"/>.
      /// </summary>
      public SerializerType SerializerType { get; set; } = SerializerType.Json;

      /// <summary>
      /// Gets or sets the expiration configuration for the cached item.
      /// If null, the item will not expire automatically.
      /// </summary>
      public CacheExpirationOptions? Expiration { get; set; }

      /// <summary>
      /// Gets or sets the tags associated with the cached item.
      /// Tags enable group-based cache invalidation operations.
      /// </summary>
      public string[] Tags { get; set; } = Array.Empty<string>();

      /// <summary>
      /// Gets or sets the cache region for partitioning cached data.
      /// Regions allow logical separation of cache entries.
      /// </summary>
      public string? Region { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether to throw exceptions on cache operation failures.
      /// When false, failures are handled gracefully without throwing exceptions.
      /// </summary>
      public bool ThrowOnError { get; set; } = false;
   }
}
