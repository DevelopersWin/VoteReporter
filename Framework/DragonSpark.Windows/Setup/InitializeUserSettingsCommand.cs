using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Commands;
using DragonSpark.Windows.Properties;
using Serilog;
using Serilog.Events;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace DragonSpark.Windows.Setup
{
	public class UserSettingsPathFactory : FactoryBase<ConfigurationUserLevel, FileInfo>
	{
		public static UserSettingsPathFactory Instance { get; } = new UserSettingsPathFactory();
		UserSettingsPathFactory() {}

		public override FileInfo Create( ConfigurationUserLevel parameter ) => new FileInfo( ConfigurationManager.OpenExeConfiguration( parameter ).FilePath );
	}

	public static class Defaults
	{
		public static Func<FileInfo> UserSettingsPath { get; } = UserSettingsPathFactory.Instance.Fixed( ConfigurationUserLevel.PerUserRoamingAndLocal ).Create().Sourced().Get;
	}

	public class ClearUserSettingCommand : DelegatedFixedCommand<FileInfo>
	{
		public static ClearUserSettingCommand Instance { get; } = new ClearUserSettingCommand();
		ClearUserSettingCommand() : base( DeleteFileCommand.Instance.Self, Defaults.UserSettingsPath ) {}
	}

	[ApplyAutoValidation]
	public class DeleteFileCommand : CommandBase<FileInfo>
	{
		public static DeleteFileCommand Instance { get; } = new DeleteFileCommand();
		DeleteFileCommand() : this( RetryCommand.Instance ) {}

		readonly RetryCommand retry;

		public DeleteFileCommand( RetryCommand retry )
		{
			this.retry = retry;
		}

		public override void Execute( FileInfo parameter ) => retry.Execute( parameter.Delete );
	}

	[ApplyAutoValidation]
	public class InitializeUserSettingsCommand : CommandBase<ApplicationSettingsBase>
	{
		readonly LogCommand log;
		readonly Func<FileInfo> fileSource;

		public InitializeUserSettingsCommand( ILogger logger ) : this( new LogCommand( logger ), Defaults.UserSettingsPath ) {}

		public InitializeUserSettingsCommand( LogCommand log, Func<FileInfo> fileSource ) : base( new OnlyOnceSpecification() )
		{
			this.log = log;
			this.fileSource = fileSource;
		}

		public override void Execute( ApplicationSettingsBase parameter )
		{
			var file = fileSource();
			file.Refresh();
			
			var templates = new[]
			{
				!file.Exists ? (ILoggerTemplate)new NotFound( file.FullName ) : new Upgrading( file.FullName ),
				Run( parameter, file )
			};

			templates.Each( log.Execute );
		}

		static ILoggerTemplate Run( ApplicationSettingsBase parameter, FileInfo file )
		{
			if ( !file.Exists )
			{
				var properties = parameter.Providers
										  .Cast<SettingsProvider>()
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
						return new ErrorSaving( e, file.FullName );
					}
				}
				return any ? (ILoggerTemplate)new Created( file.FullName ) : new NotSaved( parameter.GetType() );
			}

			parameter.Upgrade();
			return new Complete( file.FullName );
		}

		class ErrorSaving : ExceptionLoggerTemplate
		{
			public ErrorSaving( Exception exception, string path ) : base( exception, Resources.LoggerTemplates_ErrorSaving, path ) {}
		}

		class NotFound : LoggerTemplate
		{
			public NotFound( string path ) : base( LogEventLevel.Warning, Resources.LoggerTemplates_NotFound, path ) {}
		}

		class Created : LoggerTemplate
		{
			public Created( string path ) : base( Resources.LoggerTemplates_Created, path ) {}
		}

		class NotSaved : LoggerTemplate
		{
			public NotSaved( Type type ) : base( LogEventLevel.Warning, Resources.LoggerTemplates_NotSaved, type.AssemblyQualifiedName ) {}
		}

		class Upgrading : LoggerTemplate
		{
			public Upgrading( string path ) : base( Resources.LoggerTemplates_Upgrading, path ) {}
		}

		class Complete : LoggerTemplate
		{
			public Complete( string path ) : base( Resources.LoggerTemplates_Complete, path ) {}
		}
	}
}