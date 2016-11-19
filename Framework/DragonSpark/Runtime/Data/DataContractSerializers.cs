using DragonSpark.Application;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DragonSpark.Runtime.Data
{
	public sealed class DataContractSerializers : ParameterizedSourceBase<Type, DataContractSerializer>
	{
		public static IParameterizedSource<Type, DataContractSerializer> Default { get; } = new DataContractSerializers().ToCache();
		DataContractSerializers() : this( ApplicationTypes.Default ) {}

		readonly IEnumerable<Type> knownTypes;

		public DataContractSerializers( IEnumerable<Type> knownTypes ) : this( knownTypes as DtoTypeSource ?? new DtoTypeSource( knownTypes ) ) {}

		DataContractSerializers( DtoTypeSource knownTypes )
		{
			this.knownTypes = knownTypes;
		}

		public override DataContractSerializer Get( Type parameter ) => new DataContractSerializer( parameter, knownTypes );
	}

	/*public sealed class DataContractSerializers : ParameterizedSourceBase<Type, DataContractSerializer>
	{
		public static IParameterizedSource<Type, DataContractSerializer> Default { get; } = new DataContractSerializers().ToCache();
		DataContractSerializers() : this( KnownTypesOf.Default.GetEnumerable ) {}

		readonly Func<Type, IEnumerable<Type>> knownTypes;

		public DataContractSerializers( Func<Type, IEnumerable<Type>> knownTypes )
		{
			this.knownTypes = knownTypes;
		}

		public override DataContractSerializer Get( Type parameter ) => new DataContractSerializer( parameter, knownTypes( parameter ) );
	}*/
}