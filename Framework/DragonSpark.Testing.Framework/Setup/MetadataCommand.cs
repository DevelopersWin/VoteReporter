using DragonSpark.Application;
using Ploeh.AutoFixture;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCommand : AutoDataCommandBase
	{
		readonly static Func<MethodBase, ImmutableArray<ICustomization>> Factory = MetadataCustomizationFactory<ICustomization>.Default.Get;

		public static MetadataCommand Default { get; } = new MetadataCommand();
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
		public Application( /*IServiceProvider serviceProvider#1# ) : base( /*serviceProvider,#1# ApplicationCommands<T>.Default.Get() ) {}
	}

	public class ApplicationCommands<T> : Configuration<IEnumerable<ICommand>> where T : class, ICommand
	{
		public static ApplicationCommands<T> Default { get; } = new ApplicationCommands<T>();

		ApplicationCommands() : base( () => EnumerableEx.Return( new ApplyExportedCommandsCommand<T>() ).Concat( ApplicationCommands.Default.Get() ) ) {}
	}*/

	public interface IApplication : IApplication<AutoData> { }

	public class Application : Application<AutoData>, IApplication {}
}