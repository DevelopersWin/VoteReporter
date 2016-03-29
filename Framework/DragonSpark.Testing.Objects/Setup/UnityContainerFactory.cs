using DragonSpark.Composition;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Testing.Objects.Setup
{
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		public UnityContainerFactory() : base( new DragonSpark.Composition.ServiceProviderFactory( new AssemblyBasedConfigurationContainerFactory( Default<Assembly>.Items ).Create ).Create ) {}
	}

	/*[Export, Shared]
	public class RecordingLoggerFactory : Diagnostics.RecordingLoggerFactory
	{
		[ImportingConstructor]
		public RecordingLoggerFactory() {}

		[Export]
		public override LoggingLevelSwitch LevelSwitch => base.LevelSwitch;

		[Export]
		public override ILoggerHistory History => base.History;
	}*/

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		protected AutoDataAttribute( Func<AutoData, ApplicationBase> providerSource ) : base( providerSource ) {}
	}

	public class ServiceProviderFactory : DragonSpark.Composition.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		ServiceProviderFactory() : base( new AssemblyBasedConfigurationContainerFactory( AssemblyProvider.Instance.Create() ).Create ) {}
	}

	public class Application<T> : Framework.Setup.Application<T> where T : ICommand
	{
		public Application() : this( Default<ICommand>.Items ) {}

		public Application( IEnumerable<ICommand> commands ) : base( ServiceProviderFactory.Instance.Create(), commands ) {}
	}
}
