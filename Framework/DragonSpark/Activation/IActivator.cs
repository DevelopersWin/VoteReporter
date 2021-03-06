using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Activation
{
	public interface IActivator : IParameterizedSource<Type, object>, IServiceProvider, ISpecification<Type> {}

	public abstract class ActivatorBase : SpecificationParameterizedSource<Type, object>, IActivator
	{
		protected ActivatorBase( ISpecification<Type> specification, Func<Type, object> source ) : base( specification, source ) {}

		public object GetService( Type serviceType ) => Get( serviceType );
	}
}