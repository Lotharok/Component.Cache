using System;

namespace Component.Cache.Models
{
   /// <summary>
   /// Exception thrown when a cache backend cannot be found for the specified cache type and buffer type.
   /// </summary>
   public class CacheBackendNotFoundException : CacheException
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="CacheBackendNotFoundException"/> class.
      /// </summary>
      /// <param name="cacheType">The cache type that could not be resolved.</param>
      /// <param name="bufferType">The buffer type that could not be resolved.</param>
      public CacheBackendNotFoundException(CacheType cacheType, Type bufferType)
         : base($"No backend found for cache type '{cacheType}' and buffer type '{bufferType.Name}'")
      {
      }
   }
}
