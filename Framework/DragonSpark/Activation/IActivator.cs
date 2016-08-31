using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Activation
{
	public interface IActivator : IParameterizedSource<Type, object>, IServiceProvider {}

	public abstract class ActivatorBase : ParameterizedSourceBase<Type, object>, IActivator
	{
		public object GetService( Type serviceType ) => Get( serviceType );
	}
}