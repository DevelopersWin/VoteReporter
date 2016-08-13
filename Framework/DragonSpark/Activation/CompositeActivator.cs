using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
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

		bool ISpecification<TypeRequest>.IsSatisfiedBy( TypeRequest parameter ) => IsSatisfiedBy( parameter );
	}
}