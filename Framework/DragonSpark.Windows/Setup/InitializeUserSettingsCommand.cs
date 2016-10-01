using DragonSpark.Aspects.Specifications;
using DragonSpark.Aspects.Validation;
using DragonSpark.Commands;
using DragonSpark.Diagnostics;
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
	[ApplyAutoValidation, ApplyInverseSpecification( typeof(UserSettingsExistsSpecification) )]
	public sealed class InitializeUserSettingsCommand : CommandBase<ApplicationSettingsBase>
	{
		public static InitializeUserSettingsCommand Default { get; } = new InitializeUserSettingsCommand();
		InitializeUserSettingsCommand() : this( TemplatesFactory.DefaultNested.Fixed( SystemLogger.Default.ToScope().ToDelegate() ).Get, Defaults.UserSettingsPath ) {}

		readonly Func<Templates> templatesSource;
		readonly Func<FileInfo> fileSource;

		InitializeUserSettingsCommand( Func<Templates> templatesSource, Func<FileInfo> fileSource )
		{
			this.templatesSource = templatesSource;
			this.fileSource = fileSource;
		}

		public override void Execute( ApplicationSettingsBase parameter )
		{
			var templates = templatesSource();
			var file = fileSource();

			templates.Initializing( file.FullName );

			parameter.Upgrade();

			if ( !file.Exists )
			{
				templates.NotFound( file.FullName );

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
						templates.ErrorSaving( e, file.FullName );
						return;
					}

					templates.Created( file.FullName );
				}
				else
				{
					templates.NotSaved( parameter.GetType() );
				}
				return;
			}

			templates.Complete( file.FullName );
		}

		sealed class TemplatesFactory : ParameterizedSourceBase<ILogger, Templates>
		{
			public static TemplatesFactory DefaultNested { get; } = new TemplatesFactory();
			TemplatesFactory() {}

			public override Templates Get( ILogger parameter ) => new Templates( InitializingTemplate.Defaults.Get( parameter ).Execute,
																				 NotFoundTemplate.Defaults.Get( parameter ).Execute,
																				 ErrorSavingTemplate.Defaults.Get( parameter ).Execute,
																				 CreatedTemplate.Defaults.Get( parameter ).Execute,
																				 NotSavedTemplate.Defaults.Get( parameter ).Execute,
																				 CompleteTemplate.Defaults.Get( parameter ).Execute );

			sealed class InitializingTemplate : LogCommandBase<string>
			{
				public static IParameterizedSource<ILogger, InitializingTemplate> Defaults { get; } = new Cache<ILogger, InitializingTemplate>( logger => new InitializingTemplate( logger ) );
				InitializingTemplate( ILogger logger ) : base( logger, Resources.LoggerTemplates_Initializing ) {}
			}

			sealed class ErrorSavingTemplate : LogExceptionCommandBase<string>
			{
				public static IParameterizedSource<ILogger, ErrorSavingTemplate> Defaults { get; } = new Cache<ILogger, ErrorSavingTemplate>( logger => new ErrorSavingTemplate( logger ) );
				ErrorSavingTemplate( ILogger logger ) : base( logger.Warning, Resources.LoggerTemplates_ErrorSaving ) {}
			}

			sealed class NotFoundTemplate : LogCommandBase<string>
			{
				public static IParameterizedSource<ILogger, NotFoundTemplate> Defaults { get; } = new Cache<ILogger, NotFoundTemplate>( logger => new NotFoundTemplate( logger ) );
				NotFoundTemplate( ILogger logger ) : base( logger.Warning, Resources.LoggerTemplates_NotFound ) {}
			}

			sealed class CreatedTemplate : LogCommandBase<string>
			{
				public static IParameterizedSource<ILogger, CreatedTemplate> Defaults { get; } = new Cache<ILogger, CreatedTemplate>( logger => new CreatedTemplate( logger ) );
				CreatedTemplate( ILogger logger ) : base( logger, Resources.LoggerTemplates_Created ) {}
			}

			sealed class NotSavedTemplate : LogCommandBase<Type>
			{
				public static IParameterizedSource<ILogger, NotSavedTemplate> Defaults { get; } = new Cache<ILogger, NotSavedTemplate>( logger => new NotSavedTemplate( logger ) );
				NotSavedTemplate( ILogger logger ) : base( logger.Warning, Resources.LoggerTemplates_NotSaved ) {}
			}

			sealed class CompleteTemplate : LogCommandBase<string>
			{
				public static IParameterizedSource<ILogger, CompleteTemplate> Defaults { get; } = new Cache<ILogger, CompleteTemplate>( logger => new CompleteTemplate( logger ) );
				CompleteTemplate( ILogger logger ) : base( logger, Resources.LoggerTemplates_Complete ) {}
			}
		}

		sealed class Templates
		{
			public Templates( Action<string> initializing, Action<string> notFound, Action<Exception, string> errorSaving, Action<string> created, Action<Type> notSaved, Action<string> complete )
			{
				Initializing = initializing;
				NotFound = notFound;
				ErrorSaving = errorSaving;
				Created = created;
				NotSaved = notSaved;
				Complete = complete;
			}

			public Action<string> Initializing { get; }
			public Action<string> NotFound { get; }
			public Action<Exception, string> ErrorSaving { get; }
			public Action<string> Created { get; }
			public Action<Type> NotSaved { get; }
			public Action<string> Complete { get; }
		}
	}
}