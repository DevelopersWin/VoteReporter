using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Implementations
{
	public sealed class IntroduceGeneralizedSpecification : IntroduceInterfaceAspectBase
	{
		readonly static Func<Type, Func<object, object>> Factory = new ImplementationCache( SpecificationTypeDefinition.Default.ReferencedType ).ToCache().ToDelegate();
		
		public IntroduceGeneralizedSpecification() : this( typeof(DefaultGeneralizedSpecificationImplementation<>) ) {}

		public IntroduceGeneralizedSpecification( Type implementationType ) : this( Factory( implementationType ) ) {}

		[UsedImplicitly]
		public IntroduceGeneralizedSpecification( Func<object, object> factory ) : base( GeneralizedSpecificationTypeDefinition.Default, factory, GeneralizedSpecificationTypeDefinition.Default.ReferencedType ) {}
	}

	sealed class DefaultGeneralizedSpecificationImplementation<T> : DelegatedAdapter<T, bool>, ISpecification<object>
	{
		public DefaultGeneralizedSpecificationImplementation( ISpecification<T> implementation ) 
			: base( 
				  SourceCoercer<ICoercerAdapter>.Default.Get( implementation )?.To( DefaultCoercer ) ?? DefaultCoercer, 
				  new Adapters.SpecificationAdapter<T>( implementation ).Get ) {}

		public bool IsSatisfiedBy( object parameter ) => Get( Coerce( parameter ) );
	}
}