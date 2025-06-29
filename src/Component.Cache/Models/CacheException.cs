using System;

namespace Component.Cache.Models
{
   /// <summary>
   /// Base exception class for all cache-related exceptions.
   /// </summary>
   public class CacheException : Exception
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="CacheException"/> class with a specified error message.
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public CacheException(string message)
         : base(message)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="CacheException"/> class with a specified error message and inner exception.
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      /// <param name="innerException">The exception that is the cause of the current exception.</param>
      public CacheException(string message, Exception innerException)
         : base(message, innerException)
      {
      }
   }
}
