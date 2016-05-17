using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using System;
using System.Linq;

namespace DragonSpark.Activation
{
	public class CompositeActivator : FirstFromParameterFactory<object, object>, IActivator
	{
		public CompositeActivator( params IActivator[] activators ) 
			: base( new AnySpecification( activators.Select( activator => new DelegatedSpecification<object>( activator.CanCreate ) ).Fixed() ).Cast<object>(), activators.Select( activator => new Func<object, object>( activator.Create ) ).ToArray() ) {}

		bool IFactory<TypeRequest, object>.CanCreate( TypeRequest parameter ) => CanCreate( parameter );

		object IFactory<TypeRequest, object>.Create( TypeRequest parameter ) => Create( parameter );
	}
}