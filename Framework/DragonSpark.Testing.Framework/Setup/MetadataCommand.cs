using DragonSpark.Configuration;
using DragonSpark.Setup;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCommand : AutoDataCommandBase
	{
		public static MetadataCommand Instance { get; } = new MetadataCommand();
		MetadataCommand() : this( Factory ) {}

		readonly static Func<MethodBase, ICustomization[]> Factory = MetadataCustomizationFactory.Instance.Create;

		readonly Func<MethodBase, ICustomization[]> factory;
		
		public MetadataCommand( Func<MethodBase, ICustomization[]> factory )
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

	public class Application<T> : Application where T : class, ICommand
	{
		public Application() : base( ApplicationCommands<T>.Instance.Get() ) {}
	}

	public class ApplicationCommands<T> : Configuration<IEnumerable<ICommand>> where T : class, ICommand
	{
		public static ApplicationCommands<T> Instance { get; } = new ApplicationCommands<T>();

		ApplicationCommands() : base( () => EnumerableEx.Return( new ApplyExportedCommandsCommand<T>() ).Concat( ApplicationCommands.Instance.Get() ) ) {}
	}

	public class ApplicationCommands : Configuration<IEnumerable<ICommand>>
	{
		public static ApplicationCommands Instance { get; } = new ApplicationCommands();
		ApplicationCommands() : base( () => EnumerableEx.Return( MetadataCommand.Instance ) ) {}
	}

	public interface IApplication : IApplication<AutoData> { }

	public class Application : DragonSpark.Setup.Application<AutoData>, IApplication
	{
		public Application() : this( ApplicationCommands.Instance.Get() ) {}

		protected Application( IEnumerable<ICommand> commands ) : base( commands ) {}
	}
}