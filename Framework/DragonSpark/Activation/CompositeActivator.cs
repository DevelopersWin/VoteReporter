using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Linq;

namespace DragonSpark.Activation
{
	public class CompositeActivator : CompositeFactory<object, object>, IActivator
	{
		public CompositeActivator( params IActivator[] activators ) 
			: this( new AnySpecification( activators.Select( activator => activator.Cast<object>() ).ToArray() ), activators ) {}

		public CompositeActivator( ISpecification<object> specification, params IActivator[] activators ) 
			: base( specification, activators.Cast<IParameterizedSource>().Select( activator => activator.ToSourceDelegate() ).ToArray() ) {}

		object IParameterizedSource<TypeRequest, object>.Get( TypeRequest parameter ) => Get( parameter );
		object IParameterizedSource<Type, object>.Get( Type parameter ) => Get( parameter );

		bool ISpecification<TypeRequest>.IsSatisfiedBy( TypeRequest parameter ) => IsSatisfiedBy( parameter );
		bool ISpecification<Type>.IsSatisfiedBy( Type parameter ) => IsSatisfiedBy( parameter );
	}
}