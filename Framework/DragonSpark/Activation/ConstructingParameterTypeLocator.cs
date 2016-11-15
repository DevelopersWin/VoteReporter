using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Linq;

namespace DragonSpark.Activation
{
	public sealed class ConstructingParameterTypeLocator : ParameterizedSourceBase<Type, Type>
	{
		public static ConstructingParameterTypeLocator Default { get; } = new ConstructingParameterTypeLocator();
		ConstructingParameterTypeLocator() {}

		public override Type Get( Type parameter ) => 
			InstanceConstructors.Default.Get( parameter )
				.Select( info => info.GetParameterTypes() )
				.SingleOrDefault( types => types.Length == 1 )
				.NullIfDefault()?
				.Single();
	}
}