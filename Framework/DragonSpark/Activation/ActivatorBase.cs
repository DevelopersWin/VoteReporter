using System;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Activation
{
	public abstract class ActivatorBase : SpecificationParameterizedSource<Type, object>, IActivator
	{
		protected ActivatorBase( ISpecification<Type> specification, Func<Type, object> second ) : base( specification, second ) {}

		public object GetService( Type serviceType ) => Get( serviceType );
	}
}