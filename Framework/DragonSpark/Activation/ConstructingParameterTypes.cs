using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation
{
	public sealed class ConstructingParameterTypes : CacheWithImplementedFactoryBase<Type, Type>
	{
		public static ConstructingParameterTypes Default { get; } = new ConstructingParameterTypes();
		ConstructingParameterTypes() : this( InstanceConstructors.Default.Get ) {}

		readonly Func<Type, ImmutableArray<ConstructorInfo>> constructors;

		[UsedImplicitly]
		public ConstructingParameterTypes( Func<Type, ImmutableArray<ConstructorInfo>> constructors )
		{
			this.constructors = constructors;
		}

		protected override Type Create( Type parameter ) => 
			constructors( parameter )
				.Select( info => info.GetParameterTypes() )
				.SingleOrDefault( types => types.Length == 1 )
				.NullIfDefault()?
				.Single();
	}
}