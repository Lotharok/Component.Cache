# Componente Cache

[🇪🇸 Español](docs/README.es.md) | [🇺🇸 English](README.md)

Un sistema de caché flexible y extensible para .NET que proporciona múltiples backends de almacenamiento y serializadores con resolución automática de dependencias.

## 🚀 Características

- **Múltiples Backends**: Soporte para diferentes tipos de caché (InMemory, Redis, etc.)
- **Serialización Flexible**: Múltiples formatos de serialización (JSON, XML, Protobuf, MessagePack, Binary)
- **Resolución Automática**: Sistema inteligente de resolución de backends y serializadores
- **Operaciones Asíncronas**: Todas las operaciones son completamente asíncronas
- **Gestión de Errores**: Manejo de errores configurable con logging integrado
- **Operaciones Avanzadas**: Soporte para etiquetas, prefijos y expiración de caché
- **Inyección de Dependencias**: Completamente compatible con el sistema DI de .NET

## 📦 Instalación

```bash
# Instalar paquete principal
dotnet add package ChacBolay.Component.Cache

# Instalar backends
```

## ⚙️ Configuración

### Configuración Básica en Program.cs

```csharp
using Component.Cache;
using Component.Cache.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configurar Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(connectionString);
});

// Configurar opciones por defecto
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

// Registrar backends de caché
builder.Services.AddSingleton<ICacheBackend<object>, InMemoryCacheBackend>();
builder.Services.AddSingleton<ICacheBackend<string>, RedisCacheBackend>();
builder.Services.AddSingleton<ICacheBackend<byte[]>, RedisCacheBackend>();

// Registrar serializadores
builder.Services.AddSingleton<ISerializator<string>, JsonSerializator>();
builder.Services.AddSingleton<ISerializator<string>, XmlSerializator>();
builder.Services.AddSingleton<ISerializator<byte[]>, ProtobufSerializator>();
builder.Services.AddSingleton<ISerializator<byte[]>, MessagePackSerializator>();
builder.Services.AddSingleton<ISerializator<byte[]>, BinarySerializator>();

// Registrar servicios principales
builder.Services.AddSingleton<ICacheBackendResolver, CacheBackendResolver>();
builder.Services.AddScoped<ICacheService, CacheService>();

var app = builder.Build();
```

## 🔧 Uso Básico

### Inyección de Servicio

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

### Operaciones CRUD

```csharp
// Obtener valor del caché
var product = await _cacheService.GetAsync<Product>("product:123");

// Establecer valor en caché
await _cacheService.SetAsync("product:123", product);

// Obtener o establecer (patrón cache-aside)
var product = await _cacheService.GetOrSetAsync(
    "product:123", 
    async () => await _productRepository.GetByIdAsync(123)
);

// Verificar existencia
bool exists = await _cacheService.ExistsAsync("product:123");

// Eliminar valor
await _cacheService.RemoveAsync("product:123");
```

## 🎯 Uso Avanzado

### Configuración Personalizada por Operación

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

### Gestión por Etiquetas y Prefijos

```csharp
// Eliminar por prefijo
await _cacheService.RemoveByPrefixAsync("products:");

// Eliminar por etiquetas
await _cacheService.RemoveByTagsAsync(new[] { "catalog", "expired" });

// Limpiar todo el caché
await _cacheService.ClearAsync();
```

### Selección de Backend por Escenario

```csharp
// Desarrollo local - InMemory
var devOptions = new CacheOptions
{
    CacheType = CacheType.InMemory,
    SerializerType = SerializerType.None // Objetos nativos
};

// Producción - Redis con JSON
var prodOptions = new CacheOptions
{
    CacheType = CacheType.Redis,
    SerializerType = SerializerType.Json,
    Expiration = new CacheExpirationOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(15)
    }
};

// Alto rendimiento - Redis con Protobuf
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

## 📊 Backends Disponibles

| Tipo | Descripción | Tipos de Buffer | Dependencias | Uso Recomendado |
|------|-------------|----------------|--------------|-----------------|
| `InMemory` | Caché de memoria local de la aplicación | `object` | Ninguna | Desarrollo, aplicaciones monolíticas, datos temporales |
| `Redis` | Caché distribuido usando StackExchange.Redis | `string`, `byte[]` | `StackExchange.Redis` | Producción, aplicaciones distribuidas, escalabilidad horizontal |

## 🔄 Tipos de Serialización Soportados

| Tipo | Buffer | Características | Casos de Uso |
|------|--------|-----------------|--------------|
| `None` | `object` | Sin serialización | Objetos simples en memoria |
| `Json` | `string` | Legible para humanos, estándar web | APIs REST, depuración |
| `Xml` | `string` | Estructurado, legacy | Sistemas empresariales |
| `Protobuf` | `byte[]` | Compacto, rápido | Alto rendimiento |
| `MessagePack` | `byte[]` | Eficiente, multiplataforma | Microservicios |
| `Binary` | `byte[]` | Nativo de .NET | Objetos complejos |

## 🏗️ Arquitectura

```
┌─────────────────┐    ┌─────────────────────┐    ┌──────────────────┐
│   CacheService  │───▶│ CacheBackendResolver │───▶│  Cache Backends  │
│   (Fachada)     │    │    (Factory)        │    │  (Almacenamiento)│
└─────────────────┘    └─────────────────────┘    └──────────────────┘
         │                        │                         │
         │                        ▼                         │
         │               ┌─────────────────────┐            │
         └──────────────▶│   Serializadores    │◀───────────┘
                         │   (Transformación)  │
                         └─────────────────────┘
```

## 🔍 Manejo de Errores

El sistema incluye un manejo robusto de errores:

```csharp
// Configuración global de errores
var options = new CacheOptions 
{ 
    ThrowOnError = true  // false = registrar y continuar, true = lanzar excepción
};

// Las excepciones específicas incluyen:
// - CacheBackendNotFoundException
// - SerializatorNotFoundException
// - InvalidCastException (para conversiones de tipo)
```

## 📈 Mejores Prácticas

### 1. Estrategias de Caché

```csharp
// Patrón Cache-Aside (recomendado)
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

// Patrón Write-Through
public async Task UpdateProductAsync(Product product)
{
    await _repository.UpdateAsync(product);
    await _cacheService.SetAsync($"product:{product.Id}", product);
}
```

### 2. Convenciones de Nomenclatura de Claves

```csharp
// Usar prefijos jerárquicos
"user:123"
"user:123:profile"
"product:456:reviews"
"session:abc123:permissions"
```

### 3. Gestión de Expiración

```csharp
var options = new CacheOptions
{
    Expiration = new CacheExpirationOptions
    {
        // Expira después de X tiempo sin uso
        SlidingExpiration = TimeSpan.FromMinutes(30),
        
        // Expira en un tiempo específico
        AbsoluteExpiration = TimeSpan.FromHours(4),
        
        // Expira en una fecha específica
        AbsoluteExpirationRelativeToNow = DateTime.UtcNow.AddDays(1)
    }
};
```

## 🚨 Consideraciones de Rendimiento

### InMemory vs Redis

| Aspecto | InMemory | Redis |
|---------|----------|-------|
| **Velocidad** | ~1μs | ~1ms (red local) |
| **Escalabilidad** | Limitada por proceso | Horizontal |
| **Persistencia** | No | Configurable |
| **Memoria** | RAM del proceso | Dedicada |
| **Distribución** | No | Sí |

### Recomendaciones por Escenario

```csharp
// 🏃‍♂️ Alto rendimiento, datos temporales
var fastOptions = new CacheOptions
{
    CacheType = CacheType.InMemory,
    SerializerType = SerializerType.None
};

// 🌐 Aplicaciones distribuidas
var distributedOptions = new CacheOptions
{
    CacheType = CacheType.Redis,
    SerializerType = SerializerType.Protobuf // Más eficiente que JSON
};

// 🔍 Depuración y desarrollo
var debugOptions = new CacheOptions
{
    CacheType = CacheType.Redis,
    SerializerType = SerializerType.Json // Legible en Redis CLI
};
```

## 🧪 Pruebas

dotnet test --filter FullyQualifiedName!~IntegrationTest /p:CollectCoverage=true /p:CoverletOutputFormat="cobertura%2cjson" /p:CoverletOutput=../coverage-reports/ /p:MergeWith="../coverage-reports/coverage.json" -m:1
reportgenerator "-reports:coverage-reports\coverage.cobertura.xml;coverage-reports\coverage.net48.cobertura.xml;coverage-reports\coverage.net8.0.cobertura.xml" -targetdir:coverage-reports/html -historydir:coverage-reports/html/history -classfilters:'-Component.Cache.Models.*;'

## 🤝 Contribuir

1. Haz fork del repositorio
2. Crea una rama de característica (`git checkout -b feature/nueva-caracteristica`)
3. Confirma tus cambios (`git commit -am 'Agregar nueva característica'`)
4. Envía a la rama (`git push origin feature/nueva-caracteristica`)
5. Crea un Pull Request