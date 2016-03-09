using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Patterns.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCustomization : AutoDataCustomization
	{
		public static MetadataCustomization Instance { get; } = new MetadataCustomization();

		readonly Func<MethodBase, ICustomization[]> factory;

		public MetadataCustomization() : this( MetadataCustomizationFactory.Instance.Create ) {}

		public MetadataCustomization( Func<MethodBase, ICustomization[]> factory )
		{
			this.factory = factory;
		}

		protected override void OnInitializing( AutoData context ) => factory( context.Method ).Each( customization => customization.Customize( context.Fixture ) );
	}

	public class Application<T> : Application where T : ICommand
	{
		public Application( Assembly[] assemblies, IEnumerable<ICommand> commands ) : base( assemblies, commands.Append( new ApplyExportedCommandsCommand<T>() ) ) {}
	}

	public interface IApplication : DragonSpark.Setup.IApplication, ICommand<AutoData>, IDisposable { }

	// [Disposable( ThrowObjectDisposedException = true )]
	public class Application : DragonSpark.Setup.Application<AutoData>, IApplication
	{
		// [Child]
		readonly ICollection<IDisposable> disposables = new AdvisableCollection<IDisposable>();

		public Application( params ICommand[] commands ) : this( new AssemblyHost().Item, commands ) {}

		public Application( Assembly[] assemblies, IEnumerable<ICommand> commands ) : base( assemblies, commands ) {}

		protected override void ExecuteCore( ICommand[] commands, AutoData parameter )
		{
			disposables.AddRange( commands.Reverse().OfType<IDisposable>() );
			base.ExecuteCore( commands, parameter );
		}

		public void Dispose() => disposables.Purge().Each( disposable => disposable.Dispose() );
	}

	/*public class ApplicationCommandFactory : ApplicationCommandFactory<AutoData>
	{
		public ApplicationCommandFactory( IEnumerable<ICommand> commands ) : base( commands ) {}

		protected override IEnumerable<ICommand> DetermineContextCommands( ApplicationExecutionParameter<AutoData> parameter )
		{
			// yield return new FixedCommand(  );
			foreach ( var item in base.DetermineContextCommands( parameter ) )
			{
				yield return item;
			}
			yield return new FixedCommand(  );
		}
	}*/
}