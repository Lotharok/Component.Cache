using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Component.Cache.Contract;
using Component.Cache.Models;

namespace PT.UnitTest.Models
{
   public class CacheBackendStub<T> : ICacheBackend<T>
   {
      public CacheBackendStub(CacheType type, ICacheBackend<T> inner)
      {
         this.CacheType = type;
         this.Inner = inner;
      }

      public ICacheBackend<T> Inner { get; }

      public CacheType CacheType { get; }

      /// <inheritdoc />
      public Task<T> GetAsync(string key, CancellationToken cancellationToken = default)
      {
         throw new System.NotImplementedException();
      }

      /// <inheritdoc />
      public Task SetAsync(
         string key, T buffer, CacheExpirationOptions expiration, string[] tags, CancellationToken cancellationToken = default)
      {
         throw new System.NotImplementedException();
      }

      /// <inheritdoc />
      public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
      {
         throw new System.NotImplementedException();
      }

      /// <inheritdoc />
      public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
      {
         throw new System.NotImplementedException();
      }

      /// <inheritdoc />
      public Task RemoveByTagsAsync(string[] tags, CancellationToken cancellationToken = default)
      {
         throw new System.NotImplementedException();
      }

      /// <inheritdoc />
      public Task ClearAsync(CancellationToken cancellationToken = default)
      {
         throw new System.NotImplementedException();
      }

      /// <inheritdoc />
      public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
      {
         throw new System.NotImplementedException();
      }

      /// <inheritdoc />
      public Task<IEnumerable<string>> GetKeysAsync(string pattern = null, CancellationToken cancellationToken = default)
      {
         throw new System.NotImplementedException();
      }
   }
}
