using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace DragonSpark.Runtime
{
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
}