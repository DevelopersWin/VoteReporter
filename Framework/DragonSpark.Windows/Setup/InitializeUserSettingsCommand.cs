using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using Serilog;
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

		protected override string CreateItem()
		{
			try
			{
				var result = ConfigurationManager.OpenExeConfiguration( level ).FilePath;
				return result;
			}
			catch ( ConfigurationException e )
			{
				return e.Filename;
			}
		}
	}

	public class ClearUserSettingCommand : Command<ApplicationSettingsBase>
	{
		public static ClearUserSettingCommand Instance { get; } = new ClearUserSettingCommand();

		readonly Func<string> pathSource;

		public ClearUserSettingCommand() : this( UserSettingsPathFactory.Instance.Create ) {}

		public ClearUserSettingCommand( Func<string> pathSource )
		{
			this.pathSource = pathSource;
		}

		protected override void OnExecute( ApplicationSettingsBase parameter )
		{
			var path = pathSource();
			if ( File.Exists( path ) )
			{
				File.Delete( path );
			}
		}
	}

	public class InitializeUserSettingsCommand : Command<ApplicationSettingsBase>
	{
		readonly ILogger logger;
		readonly Func<string> pathSource;

		public InitializeUserSettingsCommand( ILogger logger ) : this( logger, UserSettingsPathFactory.Instance.Create ) {}

		public InitializeUserSettingsCommand( ILogger logger, Func<string> pathSource )
		{
			this.logger = logger;
			this.pathSource = pathSource;
		}

		protected override void OnExecute( ApplicationSettingsBase parameter )
		{
			var path = pathSource();
			if ( !File.Exists( path ) )
			{
				logger.Warning( "User setting file was not found at {Location}. Creating...", path );
				try
				{
					var properties = parameter.Providers
											  .Cast<SettingsProvider>()
											  .SelectMany( provider => provider.GetPropertyValues( parameter.Context, parameter.Properties ).Cast<SettingsPropertyValue>() )
											  .Where( property => property.Property.Attributes[typeof(UserScopedSettingAttribute)] is UserScopedSettingAttribute ).Fixed();

					if ( properties.Any() )
					{
						properties.Each( property => parameter[property.Name] = property.PropertyValue );
						parameter.Save();
						logger.Information( "User setting file created at {Location}", path );
					}
					else
					{
						logger.Warning( "Could not find a user-defined setting in setting {Type}.  User file not saved.", parameter.GetType().AssemblyQualifiedName );
					}
				}
				catch ( Exception e )
				{
					logger.Information( e, "Error occurred while creating user setting file at {Location}", path );
				}
			}
			else
			{
				logger.Information( "Found user settings file at {Location}.  Upgrading...", path );

				parameter.Upgrade();

				logger.Information( "User settings file at {Location} is up to date.", path );
			}
		}
	}
}