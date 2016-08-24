using DragonSpark.Commands;
using DragonSpark.Diagnostics.Exceptions;
using DragonSpark.Sources;
using System.IO;

namespace DragonSpark.Windows.Setup
{
	public class ClearUserSettingCommand : DelegatedFixedCommand<FileInfo>
	{
		public static ClearUserSettingCommand Default { get; } = new ClearUserSettingCommand();
		ClearUserSettingCommand() : base( DeleteFileCommand.Default.Apply( Defaults<IOException>.Retry ).Self, Defaults.UserSettingsPath ) {}
	}
}