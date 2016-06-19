using DragonSpark.Runtime.Specifications;
using System.Linq;

namespace DragonSpark.Activation
{
	public class CompositeActivator : FirstFromParameterFactory<object, object>, IActivator
	{
		readonly ISpecification<object> specification;

		public CompositeActivator( params IActivator[] activators ) : this( new AnySpecification( activators.Select( activator => new DelegatedSpecification<object>( activator.CanCreate ) ).ToArray() ).Cast<object>(), activators )
		{}

		public CompositeActivator( ISpecification<object> specification, params IActivator[] activators ) : base( activators.Cast<IFactoryWithParameter>().Select( activator => activator.ToDelegate() ).ToArray() )
		{
			this.specification = specification;
		}

		public override bool CanCreate( object parameter ) => specification.IsSatisfiedBy( parameter );

		bool IFactory<TypeRequest, object>.CanCreate( TypeRequest parameter ) => CanCreate( parameter );

		object IFactory<TypeRequest, object>.Create( TypeRequest parameter ) => Create( parameter );
	}
}