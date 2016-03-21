using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;
using System.Windows.Input;
using LoggingLevelSwitch = Serilog.Core.LoggingLevelSwitch;

namespace DragonSpark.Testing.Objects.Setup
{
	public class UnityContainerFactory : IntegratedUnityContainerFactory
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		public UnityContainerFactory() : base( Default<Assembly>.Items ) {}
	}

	/*[Export( typeof(Serilog.Core.LoggingLevelSwitch) ), Shared]
	public class LoggingLevelSwitch : Serilog.Core.LoggingLevelSwitch
	{}*/

	[Export, Shared]
	public class RecordingLoggerFactory : Diagnostics.RecordingLoggerFactory
	{
		[ImportingConstructor]
		public RecordingLoggerFactory() : base() {}

		[Export]
		public override LoggingLevelSwitch LevelSwitch => base.LevelSwitch;

		[Export]
		public override RecordingLogEventSink Sink => base.Sink;
	}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		protected AutoDataAttribute( Func<ApplicationBase> application ) : base( FixtureFactory.Instance.Create, application ) {}
	}

	public class ServiceProviderFactory : DragonSpark.Composition.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		ServiceProviderFactory() : base( new CompositionHostFactory( new Func<Assembly[]>( AssemblyProvider.Instance.Create ) ).Create ) {}
	}

	public class Application<T> : Framework.Setup.Application<T> where T : ICommand
	{
		public Application() : this( Default<ICommand>.Items ) {}

		public Application( IEnumerable<ICommand> commands ) : base( ServiceProviderFactory.Instance.Create(), commands ) {}
	}
}
