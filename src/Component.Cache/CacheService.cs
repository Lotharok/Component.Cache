using System;
using System.Threading;
using System.Threading.Tasks;
using Component.Cache.Contract;
using Component.Cache.Models;
using Microsoft.Extensions.Logging;

namespace Component.Cache
{
   /// <summary>
   /// Provides high-level cache operations with automatic serialization, deserialization, and backend resolution.
   /// This class serves as the main entry point for cache operations in the application.
   /// </summary>
   public class CacheService : ICacheService
   {
      private readonly ICacheBackendResolver resolver;
      private readonly ILogger<CacheService> logger;
      private readonly CacheOptions defaultOptions;

      /// <summary>
      /// Initializes a new instance of the <see cref="CacheService"/> class.
      /// </summary>
      /// <param name="resolver">
      /// The cache backend resolver used to obtain appropriate cache backends and serializers
      /// based on the specified cache type and serializer type in cache operations.
      /// </param>
      /// <param name="logger">
      /// The logger instance used for logging cache operations, errors, and performance metrics.
      /// </param>
      /// <param name="defaultOptions">
      /// The default cache options to use when no specific options are provided in cache operations.
      /// Can be null, in which case default values will be used for cache operations.
      /// </param>
      /// <exception cref="ArgumentNullException">
      /// Thrown when <paramref name="resolver"/> or <paramref name="logger"/> is null.
      /// </exception>
      /// <remarks>
      /// The cache service acts as a high-level facade over the cache backend infrastructure,
      /// providing automatic serialization/deserialization and consistent error handling.
      /// The default options serve as fallback configuration when operations don't specify
      /// their own cache options.
      /// </remarks>
      public CacheService(
          ICacheBackendResolver resolver,
          ILogger<CacheService> logger,
          CacheOptions defaultOptions)
      {
         this.resolver = resolver;
         this.logger = logger;
         this.defaultOptions = defaultOptions;
      }

      /// <inheritdoc />
      public async Task<T?> GetAsync<T>(string key, CacheOptions? options = null, CancellationToken cancellationToken = default)
      {
         if (string.IsNullOrWhiteSpace(key))
         {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
         }

         var cacheOptions = options ?? this.defaultOptions;

         try
         {
            if (this.IsMemoryBasedCache(cacheOptions.CacheType))
            {
               return await this.GetWithNativeObjectStorageAsync<T>(key, cacheOptions, cancellationToken);
            }

            return await this.GetWithSerializationAsync<T>(key, cacheOptions, cancellationToken);
         }
         catch (Exception ex) when (!cacheOptions.ThrowOnError)
         {
            this.logger.LogError(ex, "Error getting cache item with key {Key}", key);
            return default;
         }
      }

      /// <inheritdoc />
      public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default)
      {
         if (string.IsNullOrWhiteSpace(key))
         {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
         }

         if (factory == null)
         {
            throw new ArgumentNullException(nameof(factory));
         }

         var cachedValue = await this.GetAsync<T>(key, options, cancellationToken);
         if (cachedValue != null)
         {
            return cachedValue;
         }

         var value = await factory();
         await this.SetAsync(key, value, options, cancellationToken);
         return value;
      }

      /// <inheritdoc />
      public async Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default)
      {
         if (string.IsNullOrWhiteSpace(key))
         {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
         }

         var cacheOptions = options ?? this.defaultOptions;

         try
         {
            if (this.IsMemoryBasedCache(cacheOptions.CacheType))
            {
               await this.SetWithNativeObjectStorageAsync(key, value, cacheOptions, cancellationToken);
               return;
            }

            await this.SetWithSerializationAsync(key, value, cacheOptions, cancellationToken);
         }
         catch (Exception ex) when (!cacheOptions.ThrowOnError)
         {
            this.logger.LogError(ex, "Error setting cache item with key {Key}", key);
         }
      }

      /// <inheritdoc />
      public async Task RemoveAsync(string key, CacheOptions? options = null, CancellationToken cancellationToken = default)
      {
         if (string.IsNullOrWhiteSpace(key))
         {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
         }

         var cacheOptions = options ?? this.defaultOptions;

         try
         {
            var backend = this.GetBackendForCacheType(cacheOptions);
            await backend.RemoveAsync(key, cancellationToken);
         }
         catch (Exception ex) when (!cacheOptions.ThrowOnError)
         {
            this.logger.LogError(ex, "Error removing cache item with key {Key}", key);
         }
      }

      /// <inheritdoc />
      public async Task RemoveByPrefixAsync(string prefix, CacheOptions? options = null, CancellationToken cancellationToken = default)
      {
         if (string.IsNullOrWhiteSpace(prefix))
         {
            throw new ArgumentException("Prefix cannot be null or empty", nameof(prefix));
         }

         var cacheOptions = options ?? this.defaultOptions;

         try
         {
            var backend = this.GetBackendForCacheType(cacheOptions);
            await backend.RemoveByPrefixAsync(prefix, cancellationToken);
         }
         catch (Exception ex) when (!cacheOptions.ThrowOnError)
         {
            this.logger.LogError(ex, "Error removing cache items with prefix {Prefix}", prefix);
         }
      }

      /// <inheritdoc />
      public async Task RemoveByTagsAsync(string[] tags, CacheOptions? options = null, CancellationToken cancellationToken = default)
      {
         if (tags == null || tags.Length == 0)
         {
            throw new ArgumentException("Tags cannot be null or empty", nameof(tags));
         }

         var cacheOptions = options ?? this.defaultOptions;

         try
         {
            var backend = this.GetBackendForCacheType(cacheOptions);
            await backend.RemoveByTagsAsync(tags, cancellationToken);
         }
         catch (Exception ex) when (!cacheOptions.ThrowOnError)
         {
            this.logger.LogError(ex, "Error removing cache items with tags {Tags}", string.Join(", ", tags));
         }
      }

      /// <inheritdoc />
      public async Task ClearAsync(CacheOptions? options = null, CancellationToken cancellationToken = default)
      {
         var cacheOptions = options ?? this.defaultOptions;

         try
         {
            var backend = this.GetBackendForCacheType(cacheOptions);
            await backend.ClearAsync(cancellationToken);
         }
         catch (Exception ex) when (!cacheOptions.ThrowOnError)
         {
            this.logger.LogError(ex, "Error clearing cache");
         }
      }

      /// <inheritdoc />
      public async Task<bool> ExistsAsync(string key, CacheOptions? options = null, CancellationToken cancellationToken = default)
      {
         if (string.IsNullOrWhiteSpace(key))
         {
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
         }

         var cacheOptions = options ?? this.defaultOptions;

         try
         {
            var backend = this.GetBackendForCacheType(cacheOptions);
            return await backend.ExistsAsync(key, cancellationToken);
         }
         catch (Exception ex) when (!cacheOptions.ThrowOnError)
         {
            this.logger.LogError(ex, "Error checking key existence {Key}", key);
            return false;
         }
      }

      private async Task<T?> GetWithBinarySerializationAsync<T>(string key, CacheOptions options, CancellationToken cancellationToken)
      {
         var backend = this.resolver.GetBackend<byte[]>(options.CacheType);
         var serializer = this.resolver.GetSerializator<byte[]>(options.SerializerType);
         var buffer = await backend.GetAsync(key, cancellationToken);
         return buffer == null ? default : serializer.Deserialize<T>(buffer);
      }

      private async Task<T?> GetWithTextSerializationAsync<T>(string key, CacheOptions options, CancellationToken cancellationToken)
      {
         var backend = this.resolver.GetBackend<string>(options.CacheType);
         var serializer = this.resolver.GetSerializator<string>(options.SerializerType);
         var buffer = await backend.GetAsync(key, cancellationToken);
         return buffer == null ? default : serializer.Deserialize<T>(buffer);
      }

      private async Task SetWithBinarySerializationAsync<T>(string key, T value, CacheOptions options, CancellationToken cancellationToken)
      {
         var backend = this.resolver.GetBackend<byte[]>(options.CacheType);
         var serializer = this.resolver.GetSerializator<byte[]>(options.SerializerType);
         var buffer = serializer.Serialize(value);
         var expiration = options.Expiration ?? new CacheExpirationOptions();
         var tags = options.Tags ?? Array.Empty<string>();
         await backend.SetAsync(key, buffer, expiration, tags, cancellationToken);
      }

      private async Task SetWithTextSerializationAsync<T>(string key, T value, CacheOptions options, CancellationToken cancellationToken)
      {
         var backend = this.resolver.GetBackend<string>(options.CacheType);
         var serializer = this.resolver.GetSerializator<string>(options.SerializerType);
         var buffer = serializer.Serialize(value);
         var expiration = options.Expiration ?? new CacheExpirationOptions();
         var tags = options.Tags ?? Array.Empty<string>();
         await backend.SetAsync(key, buffer, expiration, tags, cancellationToken);
      }

      private async Task<T?> GetWithNativeObjectStorageAsync<T>(string key, CacheOptions options, CancellationToken cancellationToken)
      {
         var backend = this.resolver.GetBackend<object>(options.CacheType);
         var cachedObject = await backend.GetAsync(key, cancellationToken);
         if (cachedObject is T directMatch)
         {
            return directMatch;
         }

         if (cachedObject == null)
         {
            return default;
         }

         try
         {
            return (T)cachedObject;
         }
         catch (InvalidCastException)
         {
            this.logger.LogWarning(
                "Type mismatch in cache for key {Key}. Expected {ExpectedType}, found {ActualType}",
                key, typeof(T).Name, cachedObject.GetType().Name);
            return default;
         }
      }

      private async Task SetWithNativeObjectStorageAsync<T>(string key, T value, CacheOptions options, CancellationToken cancellationToken)
      {
         if (value == null)
         {
            throw new ArgumentNullException(nameof(value), "Value cannot be null when setting cache.");
         }

         var backend = this.resolver.GetBackend<object>(options.CacheType);
         var expiration = options.Expiration ?? new CacheExpirationOptions();
         var tags = options.Tags ?? Array.Empty<string>();
         await backend.SetAsync(key, value, expiration, tags, cancellationToken);
      }

      private async Task<T?> GetWithSerializationAsync<T>(string key, CacheOptions options, CancellationToken cancellationToken)
      {
         return options.SerializerType switch
         {
            SerializerType.None => await this.GetWithNativeObjectStorageAsync<T>(key, options, cancellationToken),
            SerializerType.Json => await this.GetWithTextSerializationAsync<T>(key, options, cancellationToken),
            SerializerType.Xml => await this.GetWithTextSerializationAsync<T>(key, options, cancellationToken),
            SerializerType.Protobuf => await this.GetWithBinarySerializationAsync<T>(key, options, cancellationToken),
            SerializerType.MessagePack => await this.GetWithBinarySerializationAsync<T>(key, options, cancellationToken),
            SerializerType.Binary => await this.GetWithBinarySerializationAsync<T>(key, options, cancellationToken),
            _ => throw new NotSupportedException($"Serializer type {options.SerializerType} is not supported")
         };
      }

      private async Task SetWithSerializationAsync<T>(string key, T value, CacheOptions options, CancellationToken cancellationToken)
      {
         switch (options.SerializerType)
         {
            case SerializerType.None:
               await this.SetWithNativeObjectStorageAsync(key, value, options, cancellationToken);
               break;
            case SerializerType.Json:
            case SerializerType.Xml:
               await this.SetWithTextSerializationAsync(key, value, options, cancellationToken);
               break;
            case SerializerType.Protobuf:
            case SerializerType.MessagePack:
            case SerializerType.Binary:
               await this.SetWithBinarySerializationAsync(key, value, options, cancellationToken);
               break;
            default:
               throw new NotSupportedException($"Serializer type {options.SerializerType} is not supported");
         }
      }

      private dynamic GetBackendForCacheType(CacheOptions cacheOptions)
      {
         if (this.IsMemoryBasedCache(cacheOptions.CacheType))
         {
            return this.resolver.GetBackend<object>(cacheOptions.CacheType);
         }

         return cacheOptions.SerializerType switch
         {
            SerializerType.Json or SerializerType.Xml => this.resolver.GetBackend<string>(cacheOptions.CacheType),
            SerializerType.Protobuf or SerializerType.MessagePack or SerializerType.Binary => this.resolver.GetBackend<byte[]>(cacheOptions.CacheType),
            SerializerType.None => this.resolver.GetBackend<object>(cacheOptions.CacheType),
            _ => throw new NotSupportedException($"Serializer type {cacheOptions.SerializerType} is not supported")
         };
      }

      private bool IsMemoryBasedCache(CacheType cacheType)
      {
         return cacheType == CacheType.InMemory;
      }
   }
}
