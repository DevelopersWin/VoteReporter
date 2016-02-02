﻿using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using PostSharp.Patterns.Contracts;
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
		public IMessageLogger MessageLogger { [return: Required]get; set; }

		[BuildUp]
		protected override void OnExecute( object parameter )
		{
			Database.SetInitializer( Initializer );

			using ( var context = new TContext() )
			{
				MessageLogger.Information( "Initializing Database.", Priority.Low );
				context.Database.Initialize( true );

				var items = Installers.OrderBy( x => x.Version ).Where( x => x.ContextType == typeof(TContext) && context.Installations.Find( x.Id, x.Version.ToString() ) == null ).ToArray();

				MessageLogger.Information( $"Performing entity installation on {items.Length} installers.", Priority.Low );

				items.Each( x =>
				{
					MessageLogger.Information( $"Installing Entity Installer with ID of '{x.Id}' and version '{x.Version}'.", Priority.Low );

					x.Steps.Each( y =>
					{
						y.Execute( context );
						context.Save();
					} );
					context.Create<InstallationEntry>( y => x.MapInto( y ) );
					context.Save();
				} );
				MessageLogger.Information( "Database Initialized.", Priority.Low );
			}
		}
	}
}
