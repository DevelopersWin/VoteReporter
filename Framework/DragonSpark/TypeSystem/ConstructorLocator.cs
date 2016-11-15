using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.TypeSystem
{
	public sealed class ConstructorLocator : ParameterizedSourceCache<TypeInfo, ImmutableArray<Type>, ConstructorInfo>
	{
		public static ConstructorLocator Default { get; } = new ConstructorLocator();
		ConstructorLocator() : base( info => new ExtendedDictionaryCache<ImmutableArray<Type>, ConstructorInfo>( new DefaultImplementation( info ).Get ) ) {}

		sealed class DefaultImplementation : ParameterizedSourceBase<ImmutableArray<Type>, ConstructorInfo>
		{
			ImmutableArray<ConstructorInfo> candidates;

			public DefaultImplementation( TypeInfo typeInfo ) : this( InstanceConstructors.Default.Get( typeInfo ) ) {}

			DefaultImplementation( ImmutableArray<ConstructorInfo> candidates )
			{
				this.candidates = candidates;
			}

			public override ConstructorInfo Get( ImmutableArray<Type> parameter ) =>
				candidates
					.Introduce( parameter, tuple => CompatibleArgumentsSpecification.Default.Get( tuple.Item1 ).IsSatisfiedBy( tuple.Item2 ) )
					.SingleOrDefault();
		}
	}
}