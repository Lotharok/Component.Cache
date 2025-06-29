# Cache Component

[🇪🇸 Español](docs/README.es.md) | [🇺🇸 English](README.md)

A flexible and extensible caching system for .NET that provides multiple storage backends and serializers with automatic dependency resolution.

## 🚀 Features

- **Multiple Backends**: Support for different cache types (InMemory, Redis, etc.)
- **Flexible Serialization**: Multiple serialization formats (JSON, XML, Protobuf, MessagePack, Binary)
- **Automatic Resolution**: Intelligent backend and serializer resolution system
- **Asynchronous Operations**: All operations are fully asynchronous
- **Error Management**: Configurable error handling with integrated logging
- **Advanced Operations**: Support for tags, prefixes, and cache expiration
- **Dependency Injection**: Fully compatible with .NET's DI system

## 📦 Installation

```bash
# Install main package
dotnet add package ChacBolay.Component.Cache

# Install backends
```

## ⚙️ Configuration

### Basic Setup in Program.cs

```csharp
using Component.Cache;
using Component.Cache.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(connectionString);
});

// Configure default options
builder.Services.Configure<CacheOptions>(options =>
{
    options.CacheType = CacheType.Redis;
    options.SerializerType = SerializerType.Json;
    options.ThrowOnError = false;
    options.Expiration = new CacheExpirationOptions
    {
        AbsoluteExpiration = TimeSpan.FromMinutes(30)
    };
});

// Register cache backends
builder.Services.AddSingleton<ICacheBackend<object>, InMemoryCacheBackend>();
builder.Services.AddSingleton<ICacheBackend<string>, RedisCacheBackend>();
builder.Services.AddSingleton<ICacheBackend<byte[]>, RedisCacheBackend>();

// Register serializers
builder.Services.AddSingleton<ISerializator<string>, JsonSerializator>();
builder.Services.AddSingleton<ISerializator<string>, XmlSerializator>();
builder.Services.AddSingleton<ISerializator<byte[]>, ProtobufSerializator>();
builder.Services.AddSingleton<ISerializator<byte[]>, MessagePackSerializator>();
builder.Services.AddSingleton<ISerializator<byte[]>, BinarySerializator>();

// Register main services
builder.Services.AddSingleton<ICacheBackendResolver, CacheBackendResolver>();
builder.Services.AddScoped<ICacheService, CacheService>();

var app = builder.Build();
```

## 🔧 Basic Usage

### Service Injection

```csharp
public class ProductService
{
    private readonly ICacheService _cacheService;

    public ProductService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
}
```

### CRUD Operations

```csharp
// Get value from cache
var product = await _cacheService.GetAsync<Product>("product:123");

// Set value in cache
await _cacheService.SetAsync("product:123", product);

// Get or set (cache-aside pattern)
var product = await _cacheService.GetOrSetAsync(
    "product:123", 
    async () => await _productRepository.GetByIdAsync(123)
);

// Check existence
bool exists = await _cacheService.ExistsAsync("product:123");

// Remove value
await _cacheService.RemoveAsync("product:123");
```

## 🎯 Advanced Usage

### Custom Configuration per Operation

```csharp
var options = new CacheOptions
{
    CacheType = CacheType.Redis,
    SerializerType = SerializerType.Protobuf,
    Expiration = new CacheExpirationOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(15),
        AbsoluteExpiration = TimeSpan.FromHours(2)
    },
    Tags = new[] { "products", "catalog" },
    ThrowOnError = true
};

await _cacheService.SetAsync("product:123", product, options);
```

### Management by Tags and Prefixes

```csharp
// Remove by prefix
await _cacheService.RemoveByPrefixAsync("products:");

// Remove by tags
await _cacheService.RemoveByTagsAsync(new[] { "catalog", "expired" });

// Clear entire cache
await _cacheService.ClearAsync();
```

### Backend Selection by Scenario

```csharp
// Local development - InMemory
var devOptions = new CacheOptions
{
    CacheType = CacheType.InMemory,
    SerializerType = SerializerType.None // Native objects
};

// Production - Redis with JSON
var prodOptions = new CacheOptions
{
    CacheType = CacheType.Redis,
    SerializerType = SerializerType.Json,
    Expiration = new CacheExpirationOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(15)
    }
};

// High performance - Redis with Protobuf
var perfOptions = new CacheOptions
{
    CacheType = CacheType.Redis,
    SerializerType = SerializerType.Protobuf,
    Expiration = new CacheExpirationOptions
    {
        AbsoluteExpiration = TimeSpan.FromHours(1)
    }
};
```

## 📊 Available Backends

| Type | Description | Buffer Types | Dependencies | Recommended Usage |
|------|-------------|--------------|--------------|-------------------|
| `InMemory` | Local application memory cache | `object` | None | Development, monolithic applications, temporary data |
| `Redis` | Distributed cache using StackExchange.Redis | `string`, `byte[]` | `StackExchange.Redis` | Production, distributed applications, horizontal scalability |


## 🔄 Supported Serialization Types

| Type | Buffer | Characteristics | Use Cases |
|------|--------|-----------------|-----------|
| `None` | `object` | No serialization | Simple objects in memory |
| `Json` | `string` | Human-readable, web standard | REST APIs, debugging |
| `Xml` | `string` | Structured, legacy | Enterprise systems |
| `Protobuf` | `byte[]` | Compact, fast | High performance |
| `MessagePack` | `byte[]` | Efficient, cross-platform | Microservices |
| `Binary` | `byte[]` | .NET native | Complex objects |

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────────┐    ┌──────────────────┐
│   CacheService  │───▶│ CacheBackendResolver │───▶│  Cache Backends  │
│   (Facade)      │    │    (Factory)        │    │  (Storage)       │
└─────────────────┘    └─────────────────────┘    └──────────────────┘
         │                        │                         │
         │                        ▼                         │
         │               ┌─────────────────────┐            │
         └──────────────▶│   Serializers       │◀───────────┘
                         │   (Transformation)  │
                         └─────────────────────┘
```

## 🔍 Error Handling

The system includes robust error handling:

```csharp
// Global error configuration
var options = new CacheOptions 
{ 
    ThrowOnError = true  // false = log and continue, true = throw exception
};

// Specific exceptions include:
// - CacheBackendNotFoundException
// - SerializatorNotFoundException
// - InvalidCastException (for type conversions)
```

## 📈 Best Practices

### 1. Caching Strategies

```csharp
// Cache-Aside Pattern (recommended)
public async Task<Product> GetProductAsync(int id)
{
    return await _cacheService.GetOrSetAsync(
        $"product:{id}",
        async () => await _repository.GetByIdAsync(id),
        new CacheOptions 
        { 
            Expiration = new CacheExpirationOptions 
            { 
                SlidingExpiration = TimeSpan.FromMinutes(15) 
            }
        }
    );
}

// Write-Through Pattern
public async Task UpdateProductAsync(Product product)
{
    await _repository.UpdateAsync(product);
    await _cacheService.SetAsync($"product:{product.Id}", product);
}
```

### 2. Key Naming Conventions

```csharp
// Use hierarchical prefixes
"user:123"
"user:123:profile"
"product:456:reviews"
"session:abc123:permissions"
```

### 3. Expiration Management

```csharp
var options = new CacheOptions
{
    Expiration = new CacheExpirationOptions
    {
        // Expires after X time without use
        SlidingExpiration = TimeSpan.FromMinutes(30),
        
        // Expires at a specific time
        AbsoluteExpiration = TimeSpan.FromHours(4),
        
        // Expires at a specific date
        AbsoluteExpirationRelativeToNow = DateTime.UtcNow.AddDays(1)
    }
};
```

## 🚨 Performance Considerations

### InMemory vs Redis

| Aspect | InMemory | Redis |
|--------|----------|-------|
| **Speed** | ~1μs | ~1ms (local network) |
| **Scalability** | Limited by process | Horizontal |
| **Persistence** | No | Configurable |
| **Memory** | Process RAM | Dedicated |
| **Distribution** | No | Yes |

### Recommendations by Scenario

```csharp
// 🏃‍♂️ High performance, temporary data
var fastOptions = new CacheOptions
{
    CacheType = CacheType.InMemory,
    SerializerType = SerializerType.None
};

// 🌐 Distributed applications
var distributedOptions = new CacheOptions
{
    CacheType = CacheType.Redis,
    SerializerType = SerializerType.Protobuf // More efficient than JSON
};

// 🔍 Debugging and development
var debugOptions = new CacheOptions
{
    CacheType = CacheType.Redis,
    SerializerType = SerializerType.Json // Readable in Redis CLI
};
```

## 🧪 Testing

dotnet test --filter FullyQualifiedName!~IntegrationTest /p:CollectCoverage=true /p:CoverletOutputFormat="cobertura%2cjson" /p:CoverletOutput=../coverage-reports/ /p:MergeWith="../coverage-reports/coverage.json" -m:1
reportgenerator "-reports:coverage-reports\coverage.cobertura.xml;coverage-reports\coverage.net48.cobertura.xml;coverage-reports\coverage.net8.0.cobertura.xml" -targetdir:coverage-reports/html -historydir:coverage-reports/html/history -classfilters:'-Component.Cache.Models.*;'

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/new-feature`)
3. Commit your changes (`git commit -am 'Add new feature'`)
4. Push to the branch (`git push origin feature/new-feature`)
5. Create a Pull Request

