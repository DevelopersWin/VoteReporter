using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using System.Linq;

namespace DragonSpark.Activation
{
	public class CompositeActivator : CompositeFactory<object, object>, IActivator
	{
		public CompositeActivator( params IActivator[] activators ) 
			: this( new AnySpecification( activators.Cast<IValidatedParameterizedSource>().Select( activator => activator.ToSpecification() ).ToArray() ), activators ) {}

		public CompositeActivator( ISpecification<object> specification, params IActivator[] activators ) 
			: base( specification, activators.Cast<IParameterizedSource>().Select( activator => activator.ToSourceDelegate() ).ToArray() ) {}

		bool IValidatedParameterizedSource<TypeRequest, object>.IsValid( TypeRequest parameter ) => IsValid( parameter );

		object IParameterizedSource<TypeRequest, object>.Get( TypeRequest parameter ) => Get( parameter );
		public bool IsSatisfiedBy( TypeRequest parameter ) => IsValid( parameter );
	}
}