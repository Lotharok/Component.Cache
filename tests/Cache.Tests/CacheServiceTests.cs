using System;
using System.Threading;
using System.Threading.Tasks;
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
   public class CacheServiceTests
   {
      private readonly Mock<ICacheBackendResolver> resolverMock = new Mock<ICacheBackendResolver>();
      private readonly Mock<ILogger<CacheService>> loggerMock = new Mock<ILogger<CacheService>>();
      private readonly CancellationToken cancellationToken = CancellationToken.None;
      private CacheService service;
      private CacheOptions defaultOptions;

      public CacheServiceTests()
      {
         this.defaultOptions = new CacheOptions
         {
            CacheType = CacheType.InMemory,
            SerializerType = SerializerType.None,
            ThrowOnError = true,
            Expiration = new CacheExpirationOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) },
         };
         this.service = new CacheService(this.resolverMock.Object, this.loggerMock.Object, this.defaultOptions);
      }

      [Fact]
      public async Task GetAsync_LocalCache_ReturnsValue()
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = SerializerType.Protobuf;
         var backend = new Mock<ICacheBackend<byte[]>>();
         var serializer = new Mock<ISerializator<byte[]>>();
         backend.Setup(x => x.GetAsync("testKey", this.cancellationToken)).ReturnsAsync(default(byte[]));
         this.resolverMock.Setup(x => x.GetSerializator<byte[]>(SerializerType.Protobuf)).Returns(serializer.Object);
         this.resolverMock.Setup(x => x.GetBackend<byte[]>(CacheType.Distributed)).Returns(backend.Object);
         var result = await this.service.GetAsync<string>("testKey", this.defaultOptions, this.cancellationToken);
         Assert.Null(result);
      }

      [Fact]
      public async Task GetAsync_GlobalCache_String_NotReturnsValue()
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = SerializerType.Json;
         var backend = new Mock<ICacheBackend<string>>();
         var serializer = new Mock<ISerializator<string>>();
         backend.Setup(x => x.GetAsync("testKey", this.cancellationToken)).ReturnsAsync(default(string));
         this.resolverMock.Setup(x => x.GetBackend<string>(CacheType.Distributed)).Returns(backend.Object);
         this.resolverMock.Setup(x => x.GetSerializator<string>(SerializerType.Json)).Returns(serializer.Object);
         var result = await this.service.GetAsync<string>("testKey", this.defaultOptions, this.cancellationToken);
         Assert.Null(result);
      }

      [Fact]
      public async Task GetAsync_GlobalCache_Bytes_NotReturnsValue()
      {
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.GetAsync("testKey", this.cancellationToken)).ReturnsAsync("value");
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         var result = await this.service.GetAsync<string>("testKey", this.defaultOptions, this.cancellationToken);
         Assert.Equal("value", result);
      }

      [Fact]
      public async Task GetAsync_CannotCast_LogsWarning_ReturnsDefault()
      {
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.GetAsync("key", this.cancellationToken)).ReturnsAsync(123); // not string
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         var result = await this.service.GetAsync<string>("key");
         Assert.Null(result);
         this.loggerMock.VerifyLogging(
            "Type mismatch in cache for key key. Expected String, found Int32",
            LogLevel.Warning,
            Times.Once());
      }

      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("   ")]
      public async Task GetAsync_InvalidKey_Throws(string invalidKey)
      {
         await Assert.ThrowsAsync<ArgumentException>(() => this.service.GetAsync<string>(invalidKey));
      }

      [Fact]
      public async Task SetAsync_UsesGlobalStorage()
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = SerializerType.None;
         var backend = new Mock<ICacheBackend<object>>();
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.Distributed)).Returns(backend.Object);
         await this.service.SetAsync("testKey", "value", this.defaultOptions);
         backend.Verify(x => x.SetAsync("testKey", "value", It.IsAny<CacheExpirationOptions>(), It.IsAny<string[]>(), this.cancellationToken));
      }

      [Theory]
      [InlineData(SerializerType.Json)]
      [InlineData(SerializerType.Xml)]
      public async Task SetAsync_UsesStringSerializer(SerializerType type)
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = type;
         var serializer = new Mock<ISerializator<string>>();
         var backend = new Mock<ICacheBackend<string>>();
         this.resolverMock.Setup(x => x.GetBackend<string>(CacheType.Distributed)).Returns(backend.Object);
         this.resolverMock.Setup(x => x.GetSerializator<string>(type)).Returns(serializer.Object);
         serializer.Setup(x => x.Serialize<string>("value")).Returns("serializer");
         await this.service.SetAsync("testKey", "value", this.defaultOptions);
         backend.Verify(x => x.SetAsync("testKey", "serializer", It.IsAny<CacheExpirationOptions>(), It.IsAny<string[]>(), this.cancellationToken));
      }

      [Fact]
      public async Task SetAsync_WhitOutTags_StringSerializer()
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = SerializerType.Json;
         this.defaultOptions.Tags = null;
         this.defaultOptions.Expiration = null;
         var serializer = new Mock<ISerializator<string>>();
         var backend = new Mock<ICacheBackend<string>>();
         this.resolverMock.Setup(x => x.GetBackend<string>(CacheType.Distributed)).Returns(backend.Object);
         this.resolverMock.Setup(x => x.GetSerializator<string>(SerializerType.Json)).Returns(serializer.Object);
         serializer.Setup(x => x.Serialize<string>("value")).Returns("serializer");
         await this.service.SetAsync("testKey", "value", this.defaultOptions);
         backend.Verify(x => x.SetAsync("testKey", "serializer", It.IsAny<CacheExpirationOptions>(), It.IsAny<string[]>(), this.cancellationToken));
      }

      [Theory]
      [InlineData(SerializerType.Protobuf)]
      [InlineData(SerializerType.MessagePack)]
      [InlineData(SerializerType.Binary)]
      public async Task SetAsync_WhitOutTags_BytesSerializer(SerializerType type)
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = type;
         var serializer = new Mock<ISerializator<byte[]>>();
         var backend = new Mock<ICacheBackend<byte[]>>();
         this.resolverMock.Setup(x => x.GetBackend<byte[]>(CacheType.Distributed)).Returns(backend.Object);
         this.resolverMock.Setup(x => x.GetSerializator<byte[]>(type)).Returns(serializer.Object);
         await this.service.SetAsync("testKey", "value", this.defaultOptions);
         backend.Verify(x => x.SetAsync("testKey", It.IsAny<byte[]>(), It.IsAny<CacheExpirationOptions>(), It.IsAny<string[]>(), this.cancellationToken));
      }

      [Fact]
      public async Task SetAsync_UsesBytesSerializer()
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = SerializerType.Protobuf;
         this.defaultOptions.Tags = null;
         this.defaultOptions.Expiration = null;
         var serializer = new Mock<ISerializator<byte[]>>();
         var backend = new Mock<ICacheBackend<byte[]>>();
         this.resolverMock.Setup(x => x.GetBackend<byte[]>(CacheType.Distributed)).Returns(backend.Object);
         this.resolverMock.Setup(x => x.GetSerializator<byte[]>(SerializerType.Protobuf)).Returns(serializer.Object);
         await this.service.SetAsync("testKey", "value", this.defaultOptions);
         backend.Verify(x => x.SetAsync("testKey", It.IsAny<byte[]>(), It.IsAny<CacheExpirationOptions>(), It.IsAny<string[]>(), this.cancellationToken));
      }

      [Fact]
      public async Task SetAsync_ErrorLogged()
      {
         var backend = new Mock<ICacheBackend<object>>();
         this.defaultOptions.ThrowOnError = false;
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.Distributed)).Throws<InvalidOperationException>();
         await this.service.SetAsync("testKey", "value", this.defaultOptions);
         this.loggerMock.VerifyLogging(
            "Error setting cache item with key testKey",
            LogLevel.Error,
            Times.Once());
      }

      [Fact]
      public async Task SetAsync_UsesNativeStorage()
      {
         var backend = new Mock<ICacheBackend<object>>();
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         await this.service.SetAsync("testKey", "value");
         backend.Verify(x => x.SetAsync("testKey", "value", It.IsAny<CacheExpirationOptions>(), It.IsAny<string[]>(), this.cancellationToken));
      }

      [Fact]
      public async Task SetAsync_WhitOutTags_UsesNativeStorage()
      {
         this.defaultOptions.Tags = null;
         this.defaultOptions.Expiration = null;
         var backend = new Mock<ICacheBackend<object>>();
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         await this.service.SetAsync("testKey", "value", this.defaultOptions);
         backend.Verify(x => x.SetAsync("testKey", "value", It.IsAny<CacheExpirationOptions>(), It.IsAny<string[]>(), this.cancellationToken));
      }

      [Fact]
      public void SetWithNativeObjectStorageAsync_NullValue_Throws()
      {
         var result = Assert.ThrowsAsync<ArgumentNullException>(() => this.service.SetAsync<string>("key", null));
      }

      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("   ")]
      public async Task SetAsync_InvalidKey_Throws(string invalidKey)
      {
         await Assert.ThrowsAsync<ArgumentException>(() => this.service.SetAsync<string>(invalidKey, string.Empty));
      }

      [Theory]
      [InlineData(SerializerType.Json)]
      [InlineData(SerializerType.Xml)]
      public async Task GetAsync_TextDeserialization(SerializerType type)
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = type;
         var backend = new Mock<ICacheBackend<string>>();
         var serializer = new Mock<ISerializator<string>>();
         backend.Setup(x => x.GetAsync("key", this.cancellationToken)).ReturnsAsync("serialized");
         serializer.Setup(x => x.Deserialize<string>("serialized")).Returns("value");
         this.resolverMock.Setup(x => x.GetBackend<string>(CacheType.Distributed)).Returns(backend.Object);
         this.resolverMock.Setup(x => x.GetSerializator<string>(type)).Returns(serializer.Object);
         var result = await this.service.GetAsync<string>("key", this.defaultOptions);
         Assert.Equal("value", result);
      }

      [Theory]
      [InlineData(SerializerType.Protobuf)]
      [InlineData(SerializerType.MessagePack)]
      [InlineData(SerializerType.Binary)]
      public async Task GetAsync_ProtobufBinaryDeserialization(SerializerType type)
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = type;
         var backend = new Mock<ICacheBackend<byte[]>>();
         var serializer = new Mock<ISerializator<byte[]>>();
         backend.Setup(x => x.GetAsync("key", this.cancellationToken)).ReturnsAsync(new byte[1]);
         serializer.Setup(x => x.Deserialize<string>(It.IsAny<byte[]>())).Returns("deserialized");
         this.resolverMock.Setup(x => x.GetBackend<byte[]>(CacheType.Distributed)).Returns(backend.Object);
         this.resolverMock.Setup(x => x.GetSerializator<byte[]>(type)).Returns(serializer.Object);
         var result = await this.service.GetAsync<string>("key", this.defaultOptions);
         Assert.Equal("deserialized", result);
      }

      [Theory]
      [InlineData(SerializerType.Binary)]
      [InlineData(SerializerType.MessagePack)]
      [InlineData(SerializerType.Protobuf)]
      public async Task RemoveAsync_byte_RemovesCorrectBackend(SerializerType type)
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = type;
         var backend = Mock.Of<ICacheBackend<byte[]>>(x => x.RemoveAsync("k", this.cancellationToken) == Task.CompletedTask);
         this.resolverMock.Setup(x => x.GetBackend<byte[]>(It.IsAny<CacheType>())).Returns((ICacheBackend<byte[]>)backend);
         await this.service.RemoveAsync("k", this.defaultOptions);
      }

      [Theory]
      [InlineData(SerializerType.Xml)]
      [InlineData(SerializerType.Json)]
      public async Task RemoveAsync_string_RemovesCorrectBackend(SerializerType type)
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = type;
         var backend = Mock.Of<ICacheBackend<string>>(x => x.RemoveAsync("k", this.cancellationToken) == Task.CompletedTask);
         this.resolverMock.Setup(x => x.GetBackend<string>(It.IsAny<CacheType>())).Returns((ICacheBackend<string>)backend);
         await this.service.RemoveAsync("k", this.defaultOptions);
      }

      [Theory]
      [InlineData(CacheType.InMemory, SerializerType.Xml)]
      [InlineData(CacheType.Distributed, SerializerType.None)]
      public async Task RemoveAsync_object_RemovesCorrectBackend(CacheType cache, SerializerType type)
      {
         this.defaultOptions.CacheType = cache;
         this.defaultOptions.SerializerType = type;
         var backend = Mock.Of<ICacheBackend<object>>(x => x.RemoveAsync("k", this.cancellationToken) == Task.CompletedTask);
         this.resolverMock.Setup(x => x.GetBackend<object>(It.IsAny<CacheType>())).Returns((ICacheBackend<object>)backend);
         await this.service.RemoveAsync("k", this.defaultOptions);
      }

      [Fact]
      public async Task GetAsync_ErrorLogged_ReturnsDefault()
      {
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.ThrowOnError = false;
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.Distributed)).Throws<InvalidOperationException>();
         var result = await this.service.GetAsync<string>("key", this.defaultOptions);
         Assert.Null(result);
         this.loggerMock.VerifyLogging(
            "Error getting cache item with key key",
            LogLevel.Error,
            Times.Once());
      }

      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData(" ")]
      public async Task GetOrSetAsync_KeyIsNullOrEmpty_ThrowsArgumentException(string key)
      {
         Func<Task<string>> factory = () => Task.FromResult("value");
         var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            this.service.GetOrSetAsync(key, factory));
         Assert.Equal("key", ex.ParamName);
      }

      [Fact]
      public void GetOrSetAsync_FactoryIsNull_ThrowsArgumentNullException()
      {
         var r = Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.service.GetOrSetAsync<string>("valid-key", null));
      }

      [Fact]
      public async Task GetOrSetAsync_ValueInCache_ReturnsCached()
      {
         var key = "existing-key";
         var expected = "cached-value";
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
         this.resolverMock.Setup(x => x.GetBackend<object>(It.IsAny<CacheType>())).Returns(backend.Object);
         var result = await this.service.GetOrSetAsync(key, () => Task.FromResult("factory-value"));
         Assert.Equal(expected, result);
      }

      [Fact]
      public async Task GetOrSetAsync_CacheMiss_UsesFactoryAndSets()
      {
         var key = "missing-key";
         var factoryValue = "new-value";
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>())).ReturnsAsync((object)null);
         backend.Setup(x => x.SetAsync(key, factoryValue, It.IsAny<CacheExpirationOptions>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
         this.resolverMock.Setup(x => x.GetBackend<object>(It.IsAny<CacheType>())).Returns(backend.Object);
         var result = await this.service.GetOrSetAsync(key, () => Task.FromResult(factoryValue));
         Assert.Equal(factoryValue, result);
         backend.Verify(x => x.SetAsync(key, factoryValue, It.IsAny<CacheExpirationOptions>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
      }

      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("   ")]
      public async Task RemoveAsync_InvalidKey_Throws(string invalidKey)
      {
         await Assert.ThrowsAsync<ArgumentException>(() => this.service.RemoveAsync(invalidKey));
      }

      [Fact]
      public async Task RemoveAsync_ValidKey_TextBackend_CallsRemoveAsync()
      {
         var key = "some-key";
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = SerializerType.Json;
         var backend = new Mock<ICacheBackend<string>>();
         this.resolverMock.Setup(x => x.GetBackend<string>(It.IsAny<CacheType>())).Returns(backend.Object);
         await this.service.RemoveAsync(key, this.defaultOptions);
         backend.Verify(b => b.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
      }

      [Fact]
      public async Task RemoveAsync_ValidKey_BinaryBackend_CallsRemoveAsync()
      {
         var key = "binary-key";
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = SerializerType.Binary;
         var backend = new Mock<ICacheBackend<byte[]>>();
         this.resolverMock.Setup(x => x.GetBackend<byte[]>(It.IsAny<CacheType>())).Returns(backend.Object);
         await this.service.RemoveAsync(key, this.defaultOptions);
         backend.Verify(b => b.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
      }

      [Fact]
      public async Task RemoveAsync_BackendThrows_ThrowOnErrorFalse_LogsError()
      {
         var key = "fail-key";
         this.defaultOptions.ThrowOnError = false;
         var backend = new Mock<ICacheBackend<string>>();
         backend.Setup(b => b.RemoveAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated failure"));
         this.resolverMock.Setup(x => x.GetBackend<string>(It.IsAny<CacheType>())).Returns(backend.Object);
         await this.service.RemoveAsync(key, this.defaultOptions);
         this.loggerMock.VerifyLogging(
            "Error removing cache item with key fail-key",
            LogLevel.Error,
            Times.Once());
      }

      [Fact]
      public async Task RemoveAsync_BackendThrows_ThrowOnErrorTrue_Throws()
      {
         var key = "error-key";
         this.defaultOptions.CacheType = CacheType.Distributed;
         this.defaultOptions.SerializerType = SerializerType.Xml;
         var backend = new Mock<ICacheBackend<string>>();
         backend.Setup(b => b.RemoveAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated failure"));
         this.resolverMock.Setup(x => x.GetBackend<string>(It.IsAny<CacheType>())).Returns(backend.Object);
         await Assert.ThrowsAsync<InvalidOperationException>(() => this.service.RemoveAsync(key));
      }

      [Fact]
      public async Task RemoveAsync_ValidKey_RemovesItem()
      {
         var backend = new Mock<ICacheBackend<object>>();
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         await this.service.RemoveAsync("key", this.defaultOptions, this.cancellationToken);
         backend.Verify(x => x.RemoveAsync("key", this.cancellationToken), Times.Once);
      }

      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("   ")]
      public async Task RemoveByPrefixAsync_InvalidPrefix_Throws(string invalidKey)
      {
         await Assert.ThrowsAsync<ArgumentException>(() => this.service.RemoveByPrefixAsync(invalidKey));
      }

      [Fact]
      public async Task RemoveByPrefixAsync_ValidPrefix_RemovesItems()
      {
         var backend = new Mock<ICacheBackend<object>>();
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         await this.service.RemoveByPrefixAsync("prefix", this.defaultOptions, this.cancellationToken);
         backend.Verify(x => x.RemoveByPrefixAsync("prefix", this.cancellationToken), Times.Once);
      }

      [Fact]
      public async Task RemoveByPrefixAsync_ExceptionWithoutThrowOnError_LogsError()
      {
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.RemoveByPrefixAsync("prefix", this.cancellationToken)).ThrowsAsync(new Exception("fail"));
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         this.defaultOptions.ThrowOnError = false;
         await this.service.RemoveByPrefixAsync("prefix", this.defaultOptions, this.cancellationToken);
         this.loggerMock.VerifyLogging(
            "Error removing cache items with prefix prefix",
            LogLevel.Error,
            Times.Once());
      }

      [Fact]
      public async Task RemoveByTagsAsync_ValidTags_RemovesItems()
      {
         var backend = new Mock<ICacheBackend<object>>();
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         var tags = new[] { "tag1", "tag2" };
         await this.service.RemoveByTagsAsync(tags);
         backend.Verify(x => x.RemoveByTagsAsync(tags, this.cancellationToken), Times.Once);
      }

      [Fact]
      public async Task RemoveByTagsAsync_ExceptionWithoutThrowOnError_LogsError()
      {
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.RemoveByTagsAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("fail"));
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         var tags = new[] { "tagX" };
         this.defaultOptions.ThrowOnError = false;
         await this.service.RemoveByTagsAsync(tags, this.defaultOptions, this.cancellationToken);
         this.loggerMock.VerifyLogging(
            "Error removing cache items with tags tagX",
            LogLevel.Error,
            Times.Once());
      }

      [Fact]
      public async Task RemoveByTagsAsync_NullTags_ThrowsArgumentException()
      {
         await Assert.ThrowsAsync<ArgumentException>(() => this.service.RemoveByTagsAsync(null));
      }

      [Fact]
      public async Task RemoveByTagsAsync_EmptyTags_ThrowsArgumentException()
      {
         await Assert.ThrowsAsync<ArgumentException>(() => this.service.RemoveByTagsAsync(Array.Empty<string>()));
      }

      [Fact]
      public async Task RemoveByTagsAsync_CancellationTokenCancelled_LogsAndReturns()
      {
         // Arrange
         using var cts = new CancellationTokenSource();
         cts.Cancel(); // Simula que ya está cancelado

         var backendMock = new Mock<ICacheBackend<object>>();
         backendMock
            .Setup(x => x.RemoveByTagsAsync(It.IsAny<string[]>(), cts.Token))
            .ThrowsAsync(new OperationCanceledException());

         this.resolverMock
            .Setup(x => x.GetBackend<object>(CacheType.InMemory))
            .Returns(backendMock.Object);

         var tags = new[] { "a", "b" };
         this.defaultOptions.ThrowOnError = false;

         // Act
         var act = async () => await this.service.RemoveByTagsAsync(tags, this.defaultOptions, cts.Token);

         // Assert: NO exception debe lanzarse
         await act();
         this.loggerMock.VerifyLogging(
            "Error removing cache items with tags a, b",
            LogLevel.Error,
            Times.Once());
      }

      [Fact]
      public async Task ClearAsync_ValidOptions_ClearsBackend()
      {
         var backend = new Mock<ICacheBackend<object>>();
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         await this.service.ClearAsync();
         backend.Verify(x => x.ClearAsync(this.cancellationToken), Times.Once);
      }

      [Fact]
      public async Task ClearAsync_ExceptionWithoutThrowOnError_LogsError()
      {
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.ClearAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("clear failed"));
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         this.defaultOptions.ThrowOnError = false;
         await this.service.ClearAsync(this.defaultOptions, this.cancellationToken);
         this.loggerMock.VerifyLogging(
            "Error clearing cache",
            LogLevel.Error,
            Times.Once());
      }

      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("   ")]
      public async Task ExistsAsync_InvalidKey_ThrowsArgumentException(string key)
      {
         var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            this.service.ExistsAsync(key, this.defaultOptions, this.cancellationToken));

         Assert.Equal("Key cannot be null or empty (Parameter 'key')", ex.Message);
      }

      [Fact]
      public async Task ExistsAsync_KeyExists_ReturnsTrue()
      {
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.ExistsAsync("key1", this.cancellationToken)).ReturnsAsync(true);
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         var result = await this.service.ExistsAsync("key1", this.defaultOptions, this.cancellationToken);
         Assert.True(result);
      }

      [Fact]
      public async Task ExistsAsync_DefaultOptions_KeyExists_ReturnsTrue()
      {
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.ExistsAsync("key1", this.cancellationToken)).ReturnsAsync(true);
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         var result = await this.service.ExistsAsync("key1");
         Assert.True(result);
      }

      [Fact]
      public async Task ExistsAsync_KeyNotExists_ReturnsFalse()
      {
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.ExistsAsync("key1", this.cancellationToken)).ReturnsAsync(false);
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         var result = await this.service.ExistsAsync("key1", this.defaultOptions, this.cancellationToken);
         Assert.False(result);
      }

      [Fact]
      public async Task ExistsAsync_ExceptionWithoutThrowOnError_LogsErrorAndReturnsFalse()
      {
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.ExistsAsync(It.IsAny<string>(), this.cancellationToken))
            .ThrowsAsync(new Exception("existence failed"));
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         this.defaultOptions.ThrowOnError = false;
         var result = await this.service.ExistsAsync("key1", this.defaultOptions, this.cancellationToken);
         Assert.False(result);
         this.loggerMock.VerifyLogging(
            "Error checking key existence key1",
            LogLevel.Error,
            Times.Once());
      }

      [Fact]
      public async Task ExistsAsync_ExceptionWithThrowOnError_Throws()
      {
         var backend = new Mock<ICacheBackend<object>>();
         backend.Setup(x => x.ExistsAsync(It.IsAny<string>(), this.cancellationToken))
            .ThrowsAsync(new InvalidOperationException("boom"));
         this.resolverMock.Setup(x => x.GetBackend<object>(CacheType.InMemory)).Returns(backend.Object);
         await Assert.ThrowsAsync<InvalidOperationException>(() =>
            this.service.ExistsAsync("key1", this.defaultOptions, this.cancellationToken));
      }
   }
}
