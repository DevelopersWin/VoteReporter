using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class TypeAssignableSpecification<T> : DelegatedSpecification<Type>
	{
		public static ISpecification<Type> Default { get; } = new TypeAssignableSpecification<T>();
		TypeAssignableSpecification() : base( TypeAssignableSpecification.Delegates.Get( typeof(T) ) ) {}
	}

	public sealed class TypeAssignableSpecification : SpecificationCache<Type>
	{
		public static TypeAssignableSpecification Default { get; } = new TypeAssignableSpecification();
		public static IParameterizedSource<Type, Func<Type, bool>> Delegates { get; } = new Cache<Type, Func<Type, bool>>( Default.To( SpecificationDelegates<Type>.Default ).Get );
		TypeAssignableSpecification() : base( type => new DefaultImplementation( type ).ToCachedSpecification() ) {}

		sealed class DefaultImplementation : TypeSpecificationBase
		{
			public DefaultImplementation( Type context ) : base( context ) {}

			public override bool IsSatisfiedBy( Type parameter ) =>
				Info.IsGenericTypeDefinition && parameter.ImplementsGeneric( Context ) || Info.IsAssignableFrom( parameter.GetTypeInfo() ) || Nullable.GetUnderlyingType( parameter ) == Context;
		}
	}
}