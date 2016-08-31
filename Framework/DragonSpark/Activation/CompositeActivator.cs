using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Activation
{
	public class CompositeActivator : CompositeFactory<Type, object>, IActivator
	{
		public CompositeActivator( params IActivator[] activators ) : base( activators ) {}

		public object GetService( Type serviceType ) => Get( serviceType );
	}
}