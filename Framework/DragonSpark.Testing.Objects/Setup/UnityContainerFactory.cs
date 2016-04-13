using DragonSpark.Composition;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using System.Reflection;

namespace DragonSpark.Testing.Objects.Setup
{
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		public UnityContainerFactory() : base( new ConfiguredServiceProviderFactory( Default<Assembly>.Items ).Create ) {}
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

	/*public class ServiceProviderFactory : DragonSpark.Composition.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		ServiceProviderFactory() : base( new AssemblyBasedConfigurationContainerFactory( AssemblyProvider.Instance.Create() ).Create ) {}
	}*/

	/*public class Application<T> : Framework.Setup.Application<T> where T : ICommand
	{
		public Application( IServiceProvider provider ) : this( provider, Default<ICommand>.Items ) {}

		public Application( IServiceProvider provider, IEnumerable<ICommand> commands ) : base( , commands ) {}
	}*/
}
