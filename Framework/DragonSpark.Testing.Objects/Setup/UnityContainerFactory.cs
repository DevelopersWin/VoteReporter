using DragonSpark.Runtime;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using System;
using System.Composition;
using System.Windows.Input;

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

	[Export, Shared]
	public class RecordingLoggerFactory : Diagnostics.RecordingLoggerFactory {}

	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		protected AutoDataAttribute( Func<Application> application ) : base( FixtureFactory.Instance.Create, application ) {}
	}

	public class Application<T> : Framework.Setup.Application<T> where T : ICommand
	{
		public Application( params ICommand<AutoData>[] commands ) : base( AssemblyProvider.Instance.Create(), commands ) {}
	}

	/*public class LocationBasedApplicationCustomization<T> : ApplicationCustomization where T : ICommand
	{
		public static ApplicationFactory<T> Instance { get; } = new ApplicationFactory<T>( new AssignLocationCommand() );

		public LocationBasedApplicationCustomization() : base( Instance.Create ) {}
	}*/

	/*public class ApplicationCustomization<T> : ApplicationCustomization where T : ICommand
	{
		public ApplicationCustomization() : base( ApplicationFactory<T>.Instance.Create ) {}
	}

	public class ApplicationFactory<T> : Framework.Setup.ApplicationFactory<T> where T : ICommand
	{
		public static ApplicationFactory<T> Instance { get; } = new ApplicationFactory<T>();

		public ApplicationFactory( params ICommand<object>[] commands ) : this( AssemblyProvider.Instance.Create(), commands ) {}

		protected ApplicationFactory( Assembly[] assemblies, params ICommand<object>[] commands ) : base( assemblies, commands ) {}
	}*/
}
