using Component.Cache.Models;
using Component.Serialization.Contract;

namespace Component.Cache.Contract
{
   /// <summary>
   /// Provides factory methods for resolving cache backends and serializers based on configuration.
   /// </summary>
   public interface ICacheBackendResolver
   {
      /// <summary>
      /// Resolves a cache backend instance for the specified cache type and buffer type.
      /// </summary>
      /// <typeparam name="TBuffer">The type of buffer used by the cache backend.</typeparam>
      /// <param name="type">The type of cache backend to resolve.</param>
      /// <returns>A cache backend instance.</returns>
      /// <exception cref="CacheBackendNotFoundException">Thrown when no backend is found for the specified type.</exception>
      ICacheBackend<TBuffer> GetBackend<TBuffer>(CacheType type);

      /// <summary>
      /// Resolves a serializer instance for the specified serializer type and buffer type.
      /// </summary>
      /// <typeparam name="TBuffer">The type of buffer used by the serializer.</typeparam>
      /// <param name="serializerType">The type of serializer to resolve.</param>
      /// <returns>A serializer instance.</returns>
      /// <exception cref="SerializatorNotFoundException">Thrown when no serializer is found for the specified type.</exception>
      ISerializator<TBuffer> GetSerializator<TBuffer>(SerializerType serializerType);
   }
}
