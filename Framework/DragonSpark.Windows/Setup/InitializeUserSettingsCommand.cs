using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Windows.Properties;
using Serilog;
using Serilog.Events;
using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace DragonSpark.Windows.Setup
{
	public class UserSettingsPathFactory : FactoryBase<string>
	{
		public static UserSettingsPathFactory Instance { get; } = new UserSettingsPathFactory();

		readonly ConfigurationUserLevel level;

		public UserSettingsPathFactory( ConfigurationUserLevel level = ConfigurationUserLevel.PerUserRoamingAndLocal )
		{
			this.level = level;
		}

		public override string Create() => ConfigurationManager.OpenExeConfiguration( level ).FilePath;
	}

	public class ClearUserSettingCommand : CommandBase<ApplicationSettingsBase>
	{
		public static ClearUserSettingCommand Instance { get; } = new ClearUserSettingCommand();

		readonly Func<string> pathSource;

		public ClearUserSettingCommand() : this( UserSettingsPathFactory.Instance.Create ) {}

		public ClearUserSettingCommand( Func<string> pathSource )
		{
			this.pathSource = pathSource;
		}

		public override void Execute( ApplicationSettingsBase parameter )
		{
			var path = pathSource();
			if ( File.Exists( path ) )
			{
				File.Delete( path );
			}
		}
	}

	public class InitializeUserSettingsCommand : CommandBase<ApplicationSettingsBase>
	{
		readonly LogCommand log;
		readonly Func<string> pathSource;

		public InitializeUserSettingsCommand( ILogger logger ) : this( new LogCommand( logger ), UserSettingsPathFactory.Instance.Create ) {}

		public InitializeUserSettingsCommand( LogCommand log, Func<string> pathSource ) : base( new OnlyOnceSpecification() )
		{
			this.log = log;
			this.pathSource = pathSource;
		}

		public override void Execute( ApplicationSettingsBase parameter )
		{
			var path = pathSource();
			var exists = !File.Exists( path );

			var templates = new[]
			{
				exists ? (ILoggerTemplate)new NotFound( path ) : new Upgrading( path ),
				Run( parameter, path, exists )
			};

			templates.Each( log.Run );
		}

		static ILoggerTemplate Run( ApplicationSettingsBase parameter, string path, bool exists )
		{
			if ( exists )
			{
				var properties = EnumerableExtensions.Fixed<SettingsPropertyValue>( parameter.Providers
																				  .Cast<SettingsProvider>()
																				  .SelectMany( provider => provider.GetPropertyValues( parameter.Context, parameter.Properties ).Cast<SettingsPropertyValue>() )
																				  .Where( property => property.Property.Attributes[typeof(UserScopedSettingAttribute)] is UserScopedSettingAttribute ) );
				var any = properties.Any();
				if ( any )
				{
					properties.Each( property => parameter[property.Name] = property.PropertyValue );
					try
					{
						parameter.Save();
					}
					catch ( ConfigurationErrorsException e )
					{
						return new ErrorSaving( e, path );
					}
				}
				return any ? (ILoggerTemplate)new Created( path ) : new NotSaved( parameter.GetType() );
			}

			parameter.Upgrade();
			return new Complete( path );
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