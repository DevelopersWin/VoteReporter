using DragonSpark.Activation.IoC;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Windows.Input;
using DragonSpark.Composition;
using AssemblyProvider = DragonSpark.Testing.Objects.AssemblyProvider;

namespace DragonSpark.Windows.Testing.Setup
{
	public class ServiceProviderFactory : Activation.IoC.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		ServiceProviderFactory() : base( new AssemblyBasedConfigurationContainerFactory( AssemblyProvider.Instance.Create() ).Create ) {}
	}

	public class Application<T> : DragonSpark.Testing.Framework.Setup.Application<T> where T : ICommand
	{
		public Application() : this( Default<ICommand>.Items ) {}

		public Application( IEnumerable<ICommand> commands ) : base( ServiceProviderFactory.Instance.Create(), commands ) {}
	}
}