using DragonSpark.Activation.IoC;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using System.Composition;
using System.Reflection;
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
	public class RecordingLoggerFactory : Diagnostics.RecordingLoggerFactory
	{}

	public class Customization<T> : ApplicationCustomization where T : ICommand
	{
		public Customization() : base( Factory<T>.Instance.Create ) {}
	}

	public class Factory<T> : ApplicationFactory<T> where T : ICommand
	{
		public static Factory<T> Instance { get; } = new Factory<T>();

		public Factory() : this( AssemblyProvider.Instance.Create() ) {}

		public Factory( Assembly[] assemblies ) : base( assemblies, new AssignLocationCommand() ) {}
	}
}
