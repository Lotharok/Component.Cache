using System;
using System.Collections.Generic;
using System.Linq;
using Component.Cache.Contract;
using Component.Cache.Models;
using Component.Serialization.Contract;
using Microsoft.Extensions.Logging;

namespace Component.Cache
{
   /// <summary>
   /// Resolves cache backends and serializers based on the provided cache type and serializer type.
   /// </summary>
   public class CacheBackendResolver : ICacheBackendResolver
   {
      private readonly Dictionary<CacheType, object> backends;
      private readonly Dictionary<(SerializerType, Type), object> serializators;
      private readonly ILogger<CacheBackendResolver> logger;

      /// <summary>
      /// Initializes a new instance of the <see cref="CacheBackendResolver"/> class.
      /// </summary>
      /// <param name="cacheBackends">
      /// A collection of cache backend instances that implement <see cref="ICacheBackend{TBuffer}"/>.
      /// These backends will be registered and made available for resolution based on their cache type.
      /// </param>
      /// <param name="serializators">
      /// A collection of serializer instances that implement <see cref="ISerializator{TBuffer}"/>.
      /// These serializers will be registered and made available for resolution based on their serializer type.
      /// </param>
      /// <param name="logger">
      /// The logger instance used for logging cache backend resolution operations and errors.
      /// </param>
      /// <exception cref="ArgumentNullException">
      /// Thrown when <paramref name="logger"/> is null.
      /// </exception>
      /// <remarks>
      /// The constructor initializes internal collections by processing the provided cache backends
      /// and serializers. Each backend and serializer is registered based on their respective types
      /// and buffer types for efficient lookup during resolution operations.
      /// </remarks>
      public CacheBackendResolver(
          IEnumerable<object> cacheBackends,
          IEnumerable<object> serializators,
          ILogger<CacheBackendResolver> logger)
      {
         this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
         this.backends = this.InitializeBackends(cacheBackends);
         this.serializators = this.InitializeSerializators(serializators);
      }

      /// <inheritdoc />
      public ICacheBackend<TBuffer> GetBackend<TBuffer>(CacheType type)
      {
         if (this.backends.TryGetValue(type, out var backend) && backend is ICacheBackend<TBuffer> typedBackend)
         {
            return typedBackend;
         }

         throw new CacheBackendNotFoundException(type, typeof(TBuffer));
      }

      /// <inheritdoc />
      public ISerializator<TBuffer> GetSerializator<TBuffer>(SerializerType serializerType)
      {
         if (this.serializators.TryGetValue((serializerType, typeof(TBuffer)), out var serializator) &&
             serializator is ISerializator<TBuffer> typedSerializer)
         {
            return typedSerializer;
         }

         throw new SerializatorNotFoundException(serializerType, typeof(TBuffer));
      }

      private Dictionary<CacheType, object> InitializeBackends(IEnumerable<object> cacheBackends)
      {
         var backends = new Dictionary<CacheType, object>();

         foreach (var backend in cacheBackends)
         {
            var cacheTypeProperty = backend.GetType().GetProperty("CacheType");
            if (cacheTypeProperty?.GetValue(backend) is CacheType cacheType)
            {
               backends[cacheType] = backend;
               this.logger.LogDebug(
                   "Registered backend {BackendType} for CacheType {CacheType}",
                   backend.GetType().Name, cacheType);
            }
         }

         return backends;
      }

      private Dictionary<(SerializerType, Type), object> InitializeSerializators(IEnumerable<object> serializators)
      {
         var result = new Dictionary<(SerializerType, Type), object>();

         foreach (var serializator in serializators)
         {
            var type = serializator.GetType();
            var serializatorInterface = type.GetInterfaces()
               .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISerializator<>));

            if (serializatorInterface != null)
            {
               var bufferType = serializatorInterface.GetGenericArguments()[0];
               var serializerTypeProperty = type.GetProperty("SerializerType");

               if (serializerTypeProperty?.GetValue(serializator) is SerializerType serializerType)
               {
                  result[(serializerType, bufferType)] = serializator;
                  this.logger.LogDebug(
                      "Registered serializer {SerializatorType} for {SerializerType} and buffer {BufferType}",
                      type.Name, serializerType, bufferType.Name);
               }
            }
         }

         return result;
      }
   }
}
