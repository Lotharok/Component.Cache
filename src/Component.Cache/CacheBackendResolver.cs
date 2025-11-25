using System;
using System.Collections.Generic;
using Component.Cache.Contract;
using Component.Cache.Models;
using Component.Serialization.Contract;

namespace Component.Cache
{
   /// <summary>
   /// Resolves cache backends and serializers based on the provided cache type and serializer type.
   /// </summary>
   public class CacheBackendResolver : ICacheBackendResolver
   {
      private readonly Dictionary<CacheType, object> backends;
      private readonly Dictionary<(SerializerType, Type), object> serializators;

      /// <summary>
      /// Initializes a new instance of the <see cref="CacheBackendResolver"/> class.
      /// </summary>
      /// <param name="backends">
      /// A collection of cache backend instances that implement <see cref="ICacheBackend{TBuffer}"/>.
      /// These backends will be registered and made available for resolution based on their cache type.
      /// </param>
      /// <param name="serializators">
      /// A collection of serializer instances that implement <see cref="ISerializator{TBuffer}"/>.
      /// These serializers will be registered and made available for resolution based on their serializer type.
      /// </param>
      /// <remarks>
      /// The constructor initializes internal collections by processing the provided cache backends
      /// and serializers. Each backend and serializer is registered based on their respective types
      /// and buffer types for efficient lookup during resolution operations.
      /// </remarks>
      public CacheBackendResolver(
         Dictionary<CacheType, object> backends,
         Dictionary<(SerializerType, Type), object> serializators)
      {
         this.backends = backends;
         this.serializators = serializators;
      }

      /// <inheritdoc />
      public ICacheBackend<TBuffer> GetBackend<TBuffer>(CacheType type)
      {
         if (this.backends.TryGetValue(type, out var backend) && backend is ICacheBackend<TBuffer> typed)
         {
            return typed;
         }

         throw new CacheBackendNotFoundException(type, typeof(TBuffer));
      }

      /// <inheritdoc />
      public ISerializator<TBuffer> GetSerializator<TBuffer>(SerializerType type)
      {
         if (this.serializators.TryGetValue((type, typeof(TBuffer)), out var ser) && ser is ISerializator<TBuffer> typed)
         {
            return typed;
         }

         throw new SerializatorNotFoundException(type, typeof(TBuffer));
      }
   }
}
