using System;

namespace Component.Cache.Models
{
   /// <summary>
   /// Exception thrown when a serializer cannot be found for the specified serializer type and buffer type.
   /// </summary>
   public class SerializatorNotFoundException : CacheException
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="SerializatorNotFoundException"/> class.
      /// </summary>
      /// <param name="serializerType">The serializer type that could not be resolved.</param>
      /// <param name="bufferType">The buffer type that could not be resolved.</param>
      public SerializatorNotFoundException(SerializerType serializerType, Type bufferType)
         : base($"No serializer found for serializer type '{serializerType}' and buffer type '{bufferType.Name}'")
      {
      }
   }
}
