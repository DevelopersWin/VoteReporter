using DragonSpark.Commands;
using DragonSpark.Diagnostics.Exceptions;
using System.IO;

namespace DragonSpark.Windows.Setup
{
	public class ClearUserSettingCommand : FixedCommand<FileInfo>
	{
		public static ClearUserSettingCommand Default { get; } = new ClearUserSettingCommand();
		ClearUserSettingCommand() : base( DeleteFileCommand.Default.Apply( Defaults<IOException>.Retry ), Defaults.UserSettingsPath ) {}
	}
}