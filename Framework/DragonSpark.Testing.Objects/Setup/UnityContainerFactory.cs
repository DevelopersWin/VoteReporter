using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Windows.Input;
using LoggingLevelSwitch = Serilog.Core.LoggingLevelSwitch;

namespace DragonSpark.Testing.Objects.Setup
{
	[Export, Shared]
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}
	}

	/*[Export, Shared]
	public class LoggingLevelSwitch : Serilog.Core.LoggingLevelSwitch { }*/

	[Export, Shared]
	public class RecordingLoggerFactory : Diagnostics.RecordingLoggerFactory
	{
		[ImportingConstructor]
		public RecordingLoggerFactory( RecordingLogEventSink sink, LoggingLevelSwitch levelSwitch ) : base( sink, levelSwitch ) {}
	}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		protected AutoDataAttribute( Func<Application> application ) : base( FixtureFactory.Instance.Create, application ) {}
	}

	public class Application<T> : Framework.Setup.Application<T> where T : ICommand
	{
		public Application() : this( Default<ICommand>.Items ) {}

		public Application( IEnumerable<ICommand> commands ) : base( AssemblyProvider.Instance.Create(), commands ) {}
	}
}
