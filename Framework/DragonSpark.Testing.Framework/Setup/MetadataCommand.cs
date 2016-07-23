using DragonSpark.Setup;
using Ploeh.AutoFixture;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCommand : AutoDataCommandBase
	{
		readonly static Func<MethodBase, ImmutableArray<ICustomization>> Factory = MetadataCustomizationFactory.Instance.Create;

		public static MetadataCommand Instance { get; } = new MetadataCommand();
		MetadataCommand() : this( Factory ) {}

		readonly Func<MethodBase, ImmutableArray<ICustomization>> factory;
		
		public MetadataCommand( Func<MethodBase, ImmutableArray<ICustomization>> factory )
		{
			this.factory = factory;
		}

		public override void Execute( AutoData parameter )
		{
			foreach ( var customization in factory( parameter.Method ) )
			{
				customization.Customize( parameter.Fixture );
			}
		}
	}

	/*public class Application<T> : Application where T : class, ICommand
	{
		public Application( /*IServiceProvider serviceProvider#1# ) : base( /*serviceProvider,#1# ApplicationCommands<T>.Instance.Get() ) {}
	}

	public class ApplicationCommands<T> : Configuration<IEnumerable<ICommand>> where T : class, ICommand
	{
		public static ApplicationCommands<T> Instance { get; } = new ApplicationCommands<T>();

		ApplicationCommands() : base( () => EnumerableEx.Return( new ApplyExportedCommandsCommand<T>() ).Concat( ApplicationCommands.Instance.Get() ) ) {}
	}*/

	public interface IApplication : IApplication<AutoData> { }

	public class Application : Application<AutoData>, IApplication {}
}