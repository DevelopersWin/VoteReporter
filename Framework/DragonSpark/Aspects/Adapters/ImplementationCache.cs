using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class ImplementationCache : ParameterizedSourceBase<Type, Func<object, object>>
	{
		readonly Type interfaceType;

		public ImplementationCache( Type interfaceType )
		{
			this.interfaceType = interfaceType;
		}

		public override Func<object, object> Get( Type parameter ) => new GenericAdapterFactory( interfaceType, parameter ).Get;
	}
}