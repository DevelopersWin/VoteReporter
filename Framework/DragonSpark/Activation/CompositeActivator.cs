using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using System.Linq;

namespace DragonSpark.Activation
{
	public class CompositeActivator : CompositeFactory<object, object>, IActivator
	{
		public CompositeActivator( params IActivator[] activators ) 
			: this( new AnySpecification( activators.Cast<IFactoryWithParameter>().Select( activator => activator.ToSpecification() ).ToArray() ).Cast<object>(), activators ) {}

		public CompositeActivator( ISpecification<object> specification, params IActivator[] activators ) 
			: base( specification, activators.Cast<IParameterizedSource>().Select( activator => activator.ToDelegate() ).ToArray() ) {}

		bool IFactory<TypeRequest, object>.CanCreate( TypeRequest parameter ) => CanCreate( parameter );

		object IFactory<TypeRequest, object>.Create( TypeRequest parameter ) => Create( parameter );
	}
}