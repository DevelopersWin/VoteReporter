using System;
using System.Configuration;
using System.IO;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Windows.Setup
{
	public static class Defaults
	{
		public static Func<FileInfo> UserSettingsPath { get; } = UserSettingsPathFactory.Default.Fixed( ConfigurationUserLevel.PerUserRoamingAndLocal ).ToFixedDelegate();
	}
}