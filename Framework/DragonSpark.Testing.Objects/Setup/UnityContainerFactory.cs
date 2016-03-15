using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;
using System.Windows.Input;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Testing.Framework;
using Microsoft.Practices.Unity;
using LoggingLevelSwitch = Serilog.Core.LoggingLevelSwitch;

namespace DragonSpark.Testing.Objects.Setup
{
	/*[Export, Shared]
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}
	}*/

	public class UnityContainerFactory : FactoryBase<IUnityContainer>
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		protected override IUnityContainer CreateItem()
		{
			var assemblies = new Assembly[0];
			var parameter = new ServiceLocatorFactory.Parameter( CompositionHostFactory.Instance.Create( assemblies ), assemblies );
			var result = DragonSpark.Setup.UnityContainerFactory.Instance.Create( parameter );
			return result;
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

	public class ApplicationContextFactory : DragonSpark.Setup.ApplicationContextFactory
	{
		public static ApplicationContextFactory Instance { get; } = new ApplicationContextFactory();

		ApplicationContextFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, DefaultServiceLocatorFactory.Instance.Create ) {}
	}

	public class Application<T> : Framework.Setup.Application<T> where T : ICommand
	{
		public Application() : this( Default<ICommand>.Items ) {}

		public Application( IEnumerable<ICommand> commands ) : base( ApplicationContextFactory.Instance.Create(), commands ) {}
	}
}
