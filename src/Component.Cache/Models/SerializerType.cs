namespace Component.Cache.Models
{
   /// <summary>
   /// Specifies the serialization format to use for cache operations.
   /// </summary>
   public enum SerializerType
   {
      /// <summary>
      /// No serialization format specified.
      /// </summary>
      None,

      /// <summary>
      /// JSON serialization format. Human-readable and widely supported.
      /// Best for debugging and web-based applications.
      /// </summary>
      Json,

      /// <summary>
      /// Protocol Buffers binary serialization format.
      /// Provides excellent performance and compact size with schema evolution support.
      /// </summary>
      Protobuf,

      /// <summary>
      /// MessagePack binary serialization format.
      /// Fast serialization with good size efficiency.
      /// </summary>
      MessagePack,

      /// <summary>
      /// .NET binary serialization format.
      /// Native .NET support but has security considerations.
      /// </summary>
      Binary,

      /// <summary>
      /// XML serialization format.
      /// Human-readable format suitable for configuration data and legacy system integration.
      /// </summary>
      Xml,
   }
}
