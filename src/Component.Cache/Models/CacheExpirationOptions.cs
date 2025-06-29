using System;

namespace Component.Cache.Models
{
   /// <summary>
   /// Defines the expiration behavior and priority for cached items.
   /// </summary>
   public class CacheExpirationOptions
   {
      /// <summary>
      /// Gets or sets the absolute expiration time from when the item was cached.
      /// The item will be removed from the cache after this time span.
      /// </summary>
      public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; } = TimeSpan.FromHours(2);

      /// <summary>
      /// Gets or sets the sliding expiration time. The item will be removed from the cache
      /// if it hasn't been accessed for this time span.
      /// </summary>
      public TimeSpan? SlidingExpiration { get; set; }

      /// <summary>
      /// Gets or sets the absolute expiration time relative to the current time.
      /// The item will be removed from the cache at this specific date and time.
      /// </summary>
      public DateTimeOffset? AbsoluteExpirationAt { get; set; }
   }
}
