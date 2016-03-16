using System.Collections.Generic;
using System.Windows.Input;
using DragonSpark.Composition;
using DragonSpark.Testing.Objects;
using DragonSpark.TypeSystem;

namespace DragonSpark.Windows.Testing.Setup
{
	public class ApplicationContextFactory : DragonSpark.Setup.ApplicationContextFactory
	{
		public static ApplicationContextFactory Instance { get; } = new ApplicationContextFactory();

		public ApplicationContextFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, DragonSpark.Setup.ServiceLocatorFactory.Instance.Create ) {}
	}

	public class Application<T> : DragonSpark.Testing.Framework.Setup.Application<T> where T : ICommand
	{
		public Application() : this( Default<ICommand>.Items ) {}

		public Application( IEnumerable<ICommand> commands ) : base( ApplicationContextFactory.Instance.Create(), commands ) {}
	}
}