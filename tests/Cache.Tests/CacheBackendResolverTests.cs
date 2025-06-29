using System;
using System.Linq;
using Component.Cache;
using Component.Cache.Contract;
using Component.Cache.Models;
using Component.Serialization.Contract;
using Microsoft.Extensions.Logging;
using Moq;
using PT.UnitTest.Models;
using Xunit;

namespace PT.UnitTest
{
   public class CacheBackendResolverTests
   {
      private readonly Mock<ILogger<CacheBackendResolver>> loggerMock;

      public CacheBackendResolverTests()
      {
         this.loggerMock = new Mock<ILogger<CacheBackendResolver>>();
      }

      [Fact]
      public void Constructor_ShouldThrow_WhenLoggerIsNull()
      {
         // Arrange & Act & Assert
         Assert.Throws<ArgumentNullException>(() =>
             new CacheBackendResolver(Enumerable.Empty<object>(), Enumerable.Empty<object>(), null));
      }

      [Fact]
      public void GetBackend_ShouldReturnBackend_WhenExists()
      {
         // Arrange
         var memoryBackend = new Mock<ICacheBackend<string>>();
         var backendObj = new CacheBackendStub<string>(CacheType.InMemory, memoryBackend.Object);

         var resolver = new CacheBackendResolver(
             new object[] { backendObj },
             Enumerable.Empty<object>(),
             this.loggerMock.Object);

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
             Enumerable.Empty<object>(),
             Enumerable.Empty<object>(),
             this.loggerMock.Object);

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
             Enumerable.Empty<object>(),
             new object[] { serializerObj },
             this.loggerMock.Object);

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
             Enumerable.Empty<object>(),
             Enumerable.Empty<object>(),
             this.loggerMock.Object);

         // Act & Assert
         Assert.Throws<SerializatorNotFoundException>(() =>
             resolver.GetSerializator<string>(SerializerType.Json));
      }
   }
}
