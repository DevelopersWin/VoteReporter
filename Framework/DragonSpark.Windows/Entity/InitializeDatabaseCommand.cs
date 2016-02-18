using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using PostSharp.Patterns.Contracts;
using Serilog;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Markup;

namespace DragonSpark.Windows.Entity
{
	[ContentProperty( nameof(Installers) )]
	public abstract class InitializeDatabaseCommand<TContext> : SetupCommandBase where TContext : DbContext, IEntityInstallationStorage, new()
	{
		[Locate, Required]
		public IDatabaseInitializer<TContext> Initializer { [return: Required]get; set; }

		public Collection<IInstaller> Installers { get; } = new Collection<IInstaller>();

		[Locate, Required]
		public ILogger MessageLogger { [return: Required]get; set; }

		protected override void OnExecute( object parameter )
		{
			Database.SetInitializer( Initializer );

			using ( var context = new TContext() )
			{
				MessageLogger.Information( "Initializing Database." );
				context.Database.Initialize( true );

				var items = Installers.OrderBy( x => x.Version ).Where( x => x.ContextType == typeof(TContext) && context.Installations.Find( x.Id, x.Version.ToString() ) == null ).ToArray();

				MessageLogger.Information( "Performing entity installation on {Length} installers.", items.Length );

				items.Each( x =>
				{
					MessageLogger.Information( "Installing Entity Installer with ID of '{Id}' and version '{Version}'.", x.Id, x.Version );

					x.Steps.Each( y =>
					{
						y.Execute( context );
						context.Save();
					} );
					context.Create<InstallationEntry>( y => x.MapInto( y ) );
					context.Save();
				} );
				MessageLogger.Information( "Database Initialized." );
			}
		}
	}
}
