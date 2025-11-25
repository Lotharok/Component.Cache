using System;
using System.Collections.Generic;
using Cache.Tests.Models;
using Component.Cache;
using Component.Cache.Contract;
using Component.Cache.Models;
using Component.Serialization.Contract;
using Moq;
using Xunit;

namespace Cache.Tests
{
   public class CacheBackendResolverTests
   {
      [Fact]
      public void GetBackend_ShouldReturnBackend_WhenExists()
      {
         // Arrange
         var memoryBackend = new Mock<ICacheBackend<string>>();
         var backendObj = new CacheBackendStub<string>(CacheType.InMemory, memoryBackend.Object);

         var resolver = new CacheBackendResolver(
            new Dictionary<CacheType, object> { { CacheType.InMemory, backendObj } },
            new Dictionary<(SerializerType, Type), object>());

         // Act
         var result = resolver.GetBackend<string>(CacheType.InMemory);

         // Assert
         Assert.NotNull(result);
         Assert.IsType<CacheBackendStub<string>>(result);
      }

      [Fact]
      public void GetBackend_ShouldThrow_WhenNotFound()
      {
         // Arrange
         var resolver = new CacheBackendResolver(
            new Dictionary<CacheType, object>(),
            new Dictionary<(SerializerType, Type), object>());

         // Act & Assert
         Assert.Throws<CacheBackendNotFoundException>(() =>
             resolver.GetBackend<string>(CacheType.InMemory));
      }

      [Fact]
      public void GetSerializator_ShouldReturnSerializer_WhenExists()
      {
         // Arrange
         var jsonSerializer = new Mock<ISerializator<string>>();
         var serializerObj = new SerializatorStub<string>(SerializerType.Json, jsonSerializer.Object);

         var resolver = new CacheBackendResolver(
            new Dictionary<CacheType, object>(),
            new Dictionary<(SerializerType, Type), object>
            {
               { (SerializerType.Json, typeof(string)), serializerObj },
            });

         // Act
         var result = resolver.GetSerializator<string>(SerializerType.Json);

         // Assert
         Assert.NotNull(result);
         Assert.IsType<SerializatorStub<string>>(result);
      }

      [Fact]
      public void GetSerializator_ShouldThrow_WhenNotFound()
      {
         // Arrange
         var resolver = new CacheBackendResolver(
            new Dictionary<CacheType, object>(),
            new Dictionary<(SerializerType, Type), object>());

         // Act & Assert
         Assert.Throws<SerializatorNotFoundException>(() =>
             resolver.GetSerializator<string>(SerializerType.Json));
      }
   }
}
