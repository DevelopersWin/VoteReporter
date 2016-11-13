using DragonSpark.Commands;
using DragonSpark.Windows.FileSystem;
using JetBrains.Annotations;
using System;
using System.Configuration;

namespace DragonSpark.Windows
{
	/*public sealed class SaveUserSettingsCommand : ScopedCommand<ApplicationSettingsBase>
	{
		public static SaveUserSettingsCommand Default { get; } = new SaveUserSettingsCommand();
		SaveUserSettingsCommand() : base( DefaultImplementation.Implementation.Execute ) {}
	}*/

	public sealed class SaveUserSettingsCommand : CommandBase<ApplicationSettingsBase>
	{
		public static SaveUserSettingsCommand Default { get; } = new SaveUserSettingsCommand();
		SaveUserSettingsCommand() : this( Defaults.UserSettingsPath, UserConfigurationLocator.Default.Get ) {}

		readonly Func<IFileInfo> fileSource;
		readonly Func<IFileInfo, System.Configuration.Configuration> configurationSource;

		[UsedImplicitly]
		public SaveUserSettingsCommand( Func<IFileInfo> fileSource, Func<IFileInfo, System.Configuration.Configuration> configurationSource )
		{
			this.fileSource = fileSource;
			this.configurationSource = configurationSource;
		}

		public override void Execute( ApplicationSettingsBase parameter )
		{
			var file = fileSource();
			var configuration = configurationSource( file ) ?? ConfigurationManager.OpenExeConfiguration( parameter.GetType().Assembly.Location );
			configuration.SaveAs( file.FullName, ConfigurationSaveMode.Modified );
			parameter.Save();
		}
	}
}