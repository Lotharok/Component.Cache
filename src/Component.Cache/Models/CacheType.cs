namespace Component.Cache.Models
{
   /// <summary>
   /// Specifies the type of cache backend to use for storing cached data.
   /// </summary>
   public enum CacheType
   {
      /// <summary>
      /// In-memory cache that stores data locally within the current application instance.
      /// Fast access but data is not shared across instances.
      /// </summary>
      InMemory,

      /// <summary>
      /// Global cache that can be shared across multiple application instances.
      /// Provides centralized caching for application-wide data.
      /// </summary>
      Distributed,
   }
}
