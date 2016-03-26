using System;
using System.Collections.Generic;
using System.Linq;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Activation
{
	public class CompositeActivator : FirstFromParameterFactory<object, object>, IActivator
	{
		public CompositeActivator( params IActivator[] locators ) 
			: base( locators.Select( activator => new Func<object, object>( activator.Create ) ).ToArray() ) {}

		/*class Coercer : FirstFromParameterFactory<object, TypeRequest>, IFactoryParameterCoercer<TypeRequest>
		{
			public static Coercer Instance { get; } = new Coercer();

			Coercer() : base( LocatorBase.Coercer.Instance.Coerce, ConstructorBase.Coercer.Instance.Coerce ) {}

			TypeRequest IFactoryParameterCoercer<TypeRequest>.Coerce( object context ) => Create( context );
		}*/

		object IFactory<TypeRequest, object>.Create( TypeRequest parameter ) => Create( parameter );
	}
}