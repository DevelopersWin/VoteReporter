using DragonSpark.Activation.FactoryModel;
using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;
using System.Windows.Input;
using LoggingLevelSwitch = Serilog.Core.LoggingLevelSwitch;
using ServiceLocatorFactory = DragonSpark.Setup.ServiceLocatorFactory;

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
			var parameter = new ServiceProviderParameter( CompositionHostFactory.Instance.Create( assemblies ), assemblies );
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
		protected AutoDataAttribute( Func<ApplicationBase> application ) : base( FixtureFactory.Instance.Create, application ) {}
	}

	public class ServiceProviderFactory : DragonSpark.Setup.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		ServiceProviderFactory() : base( AssemblyProvider.Instance.Create, CompositionHostFactory.Instance.Create, ServiceLocatorFactory.Instance.Create ) {}
	}

	public class Application<T> : Framework.Setup.Application<T> where T : ICommand
	{
		public Application() : this( Default<ICommand>.Items ) {}

		public Application( IEnumerable<ICommand> commands ) : base( ServiceProviderFactory.Instance.Create(), commands ) {}
	}
}
