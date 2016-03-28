using System;
using System.Linq;

namespace DragonSpark.Activation
{
	public class CompositeActivator : FirstFromParameterFactory<object, object>, IActivator
	{
		public CompositeActivator( params IActivator[] locators ) 
			: base( locators.Select( activator => new Func<object, object>( activator.Create ) ).ToArray() ) {}

		object IFactory<TypeRequest, object>.Create( TypeRequest parameter ) => Create( parameter );
	}
}