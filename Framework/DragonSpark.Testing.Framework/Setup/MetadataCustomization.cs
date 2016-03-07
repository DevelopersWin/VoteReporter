using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using Serilog;
using System;
using System.Collections.Generic;
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

	public class ApplicationCommandFactory : ApplicationCommandFactory<AutoData>
	{
		public ApplicationCommandFactory( IEnumerable<ICommand> commands ) : base( commands ) {}

		protected override IEnumerable<ICommand> DetermineContextCommands( ApplicationExecutionParameter<AutoData> parameter )
		{
			yield return new ProvisionedCommand( new AssignExecutionContextCommand(), MethodContext.Get( parameter.Arguments.Method ) );
			foreach ( var item in base.DetermineContextCommands( parameter ) )
			{
				yield return item;
			}
			yield return new ProvisionedCommand( new AssignAutoDataCommand(), parameter.Arguments );
		}
	}

	public class Application : DragonSpark.Setup.Application<AutoData>
	{
		public Application( params ICommand[] commands ) : this( Default<Assembly>.Items, commands ) {}

		public Application( Assembly[] assemblies, IEnumerable<ICommand> commands ) : base( assemblies, new ApplicationCommandFactory( commands ) ) {}

		[Compose]
		public ILogger Logger { get; set; }

		protected override void OnExecute( AutoData parameter )
		{
			Logger.Information( "Does this even work?" );
			base.OnExecute( parameter );
		}
	}
}