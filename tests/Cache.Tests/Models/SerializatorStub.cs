using System.Threading.Tasks;
using Component.Cache.Models;
using Component.Serialization.Contract;

namespace PT.UnitTest.Models
{
   public class SerializatorStub<T> : ISerializator<T>
   {
      public SerializatorStub(SerializerType serializerType, ISerializator<T> inner)
      {
         this.SerializerType = serializerType;
         this.Inner = inner;
      }

      public SerializerType SerializerType { get; }

      public ISerializator<T> Inner { get; }

      public TValue Deserialize<TValue>(T buffer)
      {
         throw new System.NotImplementedException();
      }

      /// <inheritdoc />
      public Task<T> SerializeAsync<TValue>(TValue value)
      {
         throw new System.NotImplementedException();
      }

      /// <inheritdoc />
      public Task<TValue> DeserializeAsync<TValue>(T buffer)
      {
         throw new System.NotImplementedException();
      }

      public T Serialize<TValue>(TValue value)
      {
         throw new System.NotImplementedException();
      }
   }
}
