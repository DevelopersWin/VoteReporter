using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Windows.Setup;
using DragonSpark.Windows.Testing.Properties;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Windows.Testing.Setup
{
	public class InitializeUserSettingsCommandTests : TestCollectionBase
	{
		public InitializeUserSettingsCommandTests( ITestOutputHelper output ) : base( output )
		{
			ClearUserSettingCommand.Instance.Run( Settings.Default );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void EnsureCommandRunsAsExpected( InitializeUserSettingsCommand sut, ILoggerHistory history, UserSettingsPathFactory factory )
		{
			var path = factory.Create();
			Assert.False( File.Exists( path ) );
			sut.Run( Settings.Default );

			Assert.True( File.Exists( path ) );
		}
	}
}