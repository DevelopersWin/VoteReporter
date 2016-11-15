using System;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;

namespace DragonSpark.TypeSystem
{
	public sealed class GenericImplementationSpecification : SpecificationCache<Type, Type>
	{
		public static GenericImplementationSpecification Default { get; } = new GenericImplementationSpecification();
		GenericImplementationSpecification() : this( type => new Implementation( type ) ) {}

		[UsedImplicitly]
		public GenericImplementationSpecification( Func<Type, ISpecification<Type>> create ) : base( create ) {}

		public sealed class Implementation : SpecificationWithContextBase<IParameterizedSource<Type, ImmutableArray<Type>>, Type>
		{
			public Implementation( Type referencedType ) : this( Implementations.Default.Get( referencedType ) ) {}

			[UsedImplicitly]
			public Implementation( IParameterizedSource<Type, ImmutableArray<Type>> context ) : base( context ) {}

			public override bool IsSatisfiedBy( Type parameter ) => Context.Get( parameter ).Any();
		}
	}
}