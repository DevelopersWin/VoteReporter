using DragonSpark.Composition;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Windows.Input;

namespace DragonSpark.Windows.Testing.Setup
{
	public class ApplicationServiceProviderFactory : DragonSpark.Setup.ApplicationServiceProviderFactory
	{
		public static ApplicationServiceProviderFactory Instance { get; } = new ApplicationServiceProviderFactory();

		public ApplicationServiceProviderFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, DragonSpark.Setup.ServiceLocatorFactory.Instance.Create ) {}
	}

	public class Application<T> : DragonSpark.Testing.Framework.Setup.Application<T> where T : ICommand
	{
		public Application() : this( Default<ICommand>.Items ) {}

		public Application( IEnumerable<ICommand> commands ) : base( ApplicationServiceProviderFactory.Instance.Create(), commands ) {}
	}
}