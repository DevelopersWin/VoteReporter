using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Runtime.Serialization;

namespace DragonSpark.Runtime
{
	public interface ISerializer
	{
		T Load<T>( Stream data );

		string Save<T>( T item );
	}

	public sealed class DataContractSerializers : ParameterizedSourceBase<Type, DataContractSerializer>
	{
		public static IParameterizedSource<Type, DataContractSerializer> Default { get; } = new DataContractSerializers().ToCache();
		DataContractSerializers() : this( KnownTypes.Default.Get ) {}

		readonly Func<Type, ImmutableArray<Type>> knownTypes;

		public DataContractSerializers( Func<Type, ImmutableArray<Type>> knownTypes )
		{
			this.knownTypes = knownTypes;
		}

		public override DataContractSerializer Get( Type parameter ) => new DataContractSerializer( parameter, knownTypes( parameter ).AsEnumerable() );
	}

	public sealed class Serializer : ISerializer
	{
		[Export]
		public static ISerializer Default { get; } = new Serializer();
		Serializer() {}

		public T Load<T>( Stream data ) => (T)DataContractSerializers.Default.Get( typeof(T) ).ReadObject( data );

		public string Save<T>( T item )
		{
			var stream = new MemoryStream();
			var type = typeof(T) == typeof(object) ? item.GetType() : typeof(T);
			DataContractSerializers.Default.Get( type ).WriteObject( stream, item );
			stream.Seek( 0, SeekOrigin.Begin );
			var result = new StreamReader( stream ).ReadToEnd();
			return result;
		}
	}
}