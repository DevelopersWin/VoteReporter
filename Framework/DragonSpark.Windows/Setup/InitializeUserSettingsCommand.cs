using DragonSpark.Commands;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Windows.Properties;
using Serilog;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace DragonSpark.Windows.Setup
{
	// [ApplyAutoValidation, ApplySpecification( typeof(OncePerScopeSpecification<ApplicationSettingsBase>) )]
	public sealed class InitializeUserSettingsCommand : CommandBase<ApplicationSettingsBase>
	{
		public static InitializeUserSettingsCommand Default { get; } = new InitializeUserSettingsCommand();
		InitializeUserSettingsCommand() : this( Source.DefaultNested.Get ) {}

		readonly Func<object, ICommand<ApplicationSettingsBase>> source;

		InitializeUserSettingsCommand( Func<object, ICommand<ApplicationSettingsBase>> source )
		{
			this.source = source;
		}

		public override void Execute( ApplicationSettingsBase parameter ) => source( this ).Execute( parameter );

		sealed class Source : ParameterizedSourceBase<ICommand<ApplicationSettingsBase>>
		{
			public static Source DefaultNested { get; } = new Source();
			Source() : this( SystemLogger.Default.ToScope().ToDelegate().Wrap(), Defaults.UserSettingsPath ) {}

			readonly Func<object, ILogger> loggerSource;
			readonly Func<FileInfo> fileSource;

			Source( Func<object, ILogger> loggerSource, Func<FileInfo> fileSource )
			{
				this.loggerSource = loggerSource;
				this.fileSource = fileSource;
			}

			public override ICommand<ApplicationSettingsBase> Get( object parameter ) => new Context( loggerSource( parameter ), fileSource.Refreshed() );
		}

		sealed class Context : CommandBase<ApplicationSettingsBase>
		{
			readonly FileInfo file;
			readonly Action<Exception, string> errorSaving;
			readonly Action<string> upgrading, notFound, created, complete;
			readonly Action<Type> notSaved;

			public Context( ILogger logger, FileInfo file ) : this( file,
																	Upgrading.Defaults.Get( logger ).Execute,
																	NotFound.Defaults.Get( logger ).Execute,
																	ErrorSaving.Defaults.Get( logger ).Execute,
																	Created.Defaults.Get( logger ).Execute,
																	NotSaved.Defaults.Get( logger ).Execute,
																	Complete.Defaults.Get( logger ).Execute
			) {}

			Context( FileInfo file, Action<string> upgrading, Action<string> notFound, Action<Exception, string> errorSaving, Action<string> created, Action<Type> notSaved, Action<string> complete )
			{
				this.file = file;
				this.upgrading = upgrading;
				this.notFound = notFound;
				this.errorSaving = errorSaving;
				this.created = created;
				this.notSaved = notSaved;
				this.complete = complete;
			}

			public override void Execute( ApplicationSettingsBase parameter )
			{
				var command = file.Exists ? upgrading : notFound;
				command( file.FullName );

				if ( !file.Exists )
				{
					var properties = parameter.Providers.Cast<SettingsProvider>()
											  .Introduce( parameter, tuple => tuple.Item1.GetPropertyValues( tuple.Item2.Context, tuple.Item2.Properties ).Cast<SettingsPropertyValue>() )
											  .Concat()
											  .Where( property => property.Property.Attributes[typeof(UserScopedSettingAttribute)] is UserScopedSettingAttribute ).Fixed();
					var any = properties.Any();
					if ( any )
					{
						foreach ( var property in properties )
						{
							parameter[property.Name] = property.PropertyValue;
						}

						try
						{
							parameter.Save();
						}
						catch ( ConfigurationErrorsException e )
						{
							errorSaving( e, file.FullName );
							return;
						}
					}
					if ( any )
					{
						created( file.FullName );
					}
					else
					{
						notSaved( parameter.GetType() );
					}
					return;
				}

				parameter.Upgrade();
				complete( file.FullName );
			}

			sealed class ErrorSaving : LogExceptionCommandBase<string>
			{
				public static IParameterizedSource<ILogger, ErrorSaving> Defaults { get; } = new Cache<ILogger, ErrorSaving>( logger => new ErrorSaving( logger ) );
				ErrorSaving( ILogger logger ) : base( logger, Resources.LoggerTemplates_ErrorSaving ) {}
			}

			sealed class NotFound : LogCommandBase<string>
			{
				public static IParameterizedSource<ILogger, NotFound> Defaults { get; } = new Cache<ILogger, NotFound>( logger => new NotFound( logger ) );
				NotFound( ILogger logger ) : base( logger.Warning, Resources.LoggerTemplates_NotFound ) {}
			}

			sealed class Created : LogCommandBase<string>
			{
				public static IParameterizedSource<ILogger, Created> Defaults { get; } = new Cache<ILogger, Created>( logger => new Created( logger ) );
				Created( ILogger logger ) : base( logger, Resources.LoggerTemplates_Created ) {}
			}

			sealed class NotSaved : LogCommandBase<Type>
			{
				public static IParameterizedSource<ILogger, NotSaved> Defaults { get; } = new Cache<ILogger, NotSaved>( logger => new NotSaved( logger ) );
				NotSaved( ILogger logger ) : base( logger.Warning, Resources.LoggerTemplates_NotSaved ) {}
			}

			sealed class Upgrading : LogCommandBase<string>
			{
				public static IParameterizedSource<ILogger, Upgrading> Defaults { get; } = new Cache<ILogger, Upgrading>( logger => new Upgrading( logger ) );
				Upgrading( ILogger logger ) : base( logger, Resources.LoggerTemplates_Upgrading ) {}
			}

			sealed class Complete : LogCommandBase<string>
			{
				public static IParameterizedSource<ILogger, Complete> Defaults { get; } = new Cache<ILogger, Complete>( logger => new Complete( logger ) );
				Complete( ILogger logger ) : base( logger, Resources.LoggerTemplates_Complete ) {}
			}
		}

		
	}
}