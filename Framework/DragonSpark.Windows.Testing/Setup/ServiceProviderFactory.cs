using DragonSpark.Activation.IoC;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using AssemblyProvider = DragonSpark.Testing.Objects.AssemblyProvider;

namespace DragonSpark.Windows.Testing.Setup
{
	public class ServiceProviderFactory : Activation.IoC.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		ServiceProviderFactory() : base( new IntegratedUnityContainerFactory( new Func<Assembly[]>( AssemblyProvider.Instance.Create ) ).Create ) {}
	}

	public class Application<T> : DragonSpark.Testing.Framework.Setup.Application<T> where T : ICommand
	{
		public Application() : this( Default<ICommand>.Items ) {}

		public Application( IEnumerable<ICommand> commands ) : base( ServiceProviderFactory.Instance.Create(), commands ) {}
	}
}