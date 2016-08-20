﻿using DragonSpark.Aspects.Validation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Commands;
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
	public class UserSettingsPathFactory : ParameterizedSourceBase<ConfigurationUserLevel, FileInfo>
	{
		public static UserSettingsPathFactory Instance { get; } = new UserSettingsPathFactory();
		UserSettingsPathFactory() {}

		public override FileInfo Get( ConfigurationUserLevel parameter ) => new FileInfo( ConfigurationManager.OpenExeConfiguration( parameter ).FilePath );
	}
	
	public static class Defaults
	{
		public static Func<FileInfo> UserSettingsPath { get; } = UserSettingsPathFactory.Instance.Fixed( ConfigurationUserLevel.PerUserRoamingAndLocal ).ToFixedDelegate();
	}

	public class ClearUserSettingCommand : DelegatedFixedCommand<FileInfo>
	{
		public static ClearUserSettingCommand Instance { get; } = new ClearUserSettingCommand();
		ClearUserSettingCommand() : base( DeleteFileCommand.Instance.Get, Defaults.UserSettingsPath ) {}
	}

	[ApplyAutoValidation]
	public class DeleteFileCommand : CommandBase<FileInfo>
	{
		public static ISource<ICommand<FileInfo>> Instance { get; } = new Scope<ICommand<FileInfo>>( Factory.Global( () => new DeleteFileCommand() ) );
		DeleteFileCommand() : this( RetryPolicyFactory<IOException>.Instance.ToCommand() ) {}

		readonly ICommand<Action> applyPolicy;

		public DeleteFileCommand( ICommand<Action> applyPolicy ) : base( FileSystemInfoExistsSpecification.Instance )
		{
			this.applyPolicy = applyPolicy;
		}

		public override void Execute( FileInfo parameter ) => applyPolicy.Execute( parameter.Delete );
	}

	public sealed class FileSystemInfoExistsSpecification : SpecificationBase<FileSystemInfo>
	{
		public static FileSystemInfoExistsSpecification Instance { get; } = new FileSystemInfoExistsSpecification();
		FileSystemInfoExistsSpecification() {}

		public override bool IsSatisfiedBy( FileSystemInfo parameter )
		{
			parameter.Refresh();
			return parameter.Exists;
		}
	}

	[ApplyAutoValidation]
	public class InitializeUserSettingsCommand : CommandBase<ApplicationSettingsBase>
	{
		readonly ILogger logger;
		readonly Func<FileInfo> fileSource;

		public InitializeUserSettingsCommand( ILogger logger ) : this( logger, Defaults.UserSettingsPath ) {}

		public InitializeUserSettingsCommand( ILogger logger, Func<FileInfo> fileSource ) : base( new OnlyOnceSpecification() )
		{
			this.logger = logger;
			this.fileSource = fileSource;
		}

		public override void Execute( ApplicationSettingsBase parameter )
		{
			var file = fileSource();
			file.Refresh();

			var command = file.Exists ? (ICommand<string>)Upgrading.Defaults.Get( logger ) : NotFound.Defaults.Get( logger );
			command.Execute( file.FullName );

			Run( logger, parameter, file );
		}

		static void Run( ILogger logger, ApplicationSettingsBase parameter, FileSystemInfo file )
		{
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
						ErrorSaving.Defaults.Get( logger ).Execute( e, file.FullName );
						return;
					}
				}
				if ( any )
				{
					Created.Defaults.Get( logger ).Execute( file.FullName );
				}
				else
				{
					NotSaved.Defaults.Get( logger ).Execute( parameter.GetType() );
				}
				return;
			}

			parameter.Upgrade();
			Complete.Defaults.Get( logger ).Execute( file.FullName );
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