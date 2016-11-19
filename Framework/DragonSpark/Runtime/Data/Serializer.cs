using JetBrains.Annotations;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace DragonSpark.Runtime.Data
{
	public sealed class Serializer : ISerializer
	{
		public static ISerializer Default { get; } = new Serializer();
		Serializer() : this( DataContractSerializers.Default.Get ) {}

		readonly Func<Type, DataContractSerializer> source;

		[UsedImplicitly]
		public Serializer( Func<Type, DataContractSerializer> source )
		{
			this.source = source;
		}

		public T Load<T>( Stream data ) => (T)source( typeof(T) ).ReadObject( data );

		public string Save<T>( T item )
		{
			var stream = new MemoryStream();
			var type = typeof(T) == typeof(object) ? item.GetType() : typeof(T);
			source( type ).WriteObject( stream, item );
			stream.Seek( 0, SeekOrigin.Begin );
			var result = new StreamReader( stream ).ReadToEnd();
			return result;
		}
	}
}