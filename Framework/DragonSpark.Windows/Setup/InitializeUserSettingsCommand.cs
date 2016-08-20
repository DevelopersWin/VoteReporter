using DragonSpark.Aspects.Validation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Commands;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Windows.Properties;
using Serilog;
using Serilog.Events;
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
				var properties = EnumerableExtensions.Fixed<SettingsPropertyValue>( Enumerable.Cast<SettingsProvider>( parameter.Providers )
																																																	  .Introduce( parameter, tuple => Enumerable.Cast<SettingsPropertyValue>( tuple.Item1.GetPropertyValues( tuple.Item2.Context, tuple.Item2.Properties ) ) )
																																																	  .Concat()
																																																	  .Where( property => property.Property.Attributes[typeof(UserScopedSettingAttribute)] is UserScopedSettingAttribute ) );
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