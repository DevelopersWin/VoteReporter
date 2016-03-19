using DragonSpark.Composition;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Windows.Input;
using DragonSpark.Testing.Framework.Setup;
using AssemblyProvider = DragonSpark.Testing.Objects.AssemblyProvider;

namespace DragonSpark.Windows.Testing.Setup
{
	public class ServiceProviderFactory : DragonSpark.Setup.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		public ServiceProviderFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, Activation.IoC.ServiceLocatorFactory.Instance.Create ) {}
	}

	public class Application<T> : DragonSpark.Testing.Framework.Setup.Application<T> where T : ICommand
	{
		public Application( AutoData autoData ) : this( autoData, Default<ICommand>.Items ) {}

		public Application( AutoData autoData, IEnumerable<ICommand> commands ) : base( autoData, ServiceProviderFactory.Instance.Create(), commands ) {}
	}
}