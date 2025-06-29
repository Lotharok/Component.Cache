namespace Component.Cache.Models
{
   /// <summary>
   /// Specifies the priority level of a cached item for eviction policies.
   /// </summary>
   public enum CacheItemPriority
   {
      /// <summary>
      /// Low priority items are evicted first when cache capacity is reached.
      /// </summary>
      Low,

      /// <summary>
      /// Normal priority items have standard eviction behavior.
      /// </summary>
      Normal,

      /// <summary>
      /// High priority items are evicted last when cache capacity is reached.
      /// </summary>
      High,

      /// <summary>
      /// Items that should never be automatically removed from the cache.
      /// These items can only be removed explicitly.
      /// </summary>
      NeverRemove,
   }
}
