﻿using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Windows.Setup;
using DragonSpark.Windows.Testing.Properties;
using System.Configuration;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Resources = DragonSpark.Windows.Properties.Resources;

namespace DragonSpark.Windows.Testing.Setup
{
	public class InitializeUserSettingsCommandTests : TestCollectionBase
	{
		public InitializeUserSettingsCommandTests( ITestOutputHelper output ) : base( output )
		{
			Clear();
		}

		static void Clear() => ClearUserSettingCommand.Instance.Run( Settings.Default );

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void Create( InitializeUserSettingsCommand sut, ILoggerHistory history, UserSettingsPathFactory factory )
		{
			var path = factory.Create();
			Assert.False( File.Exists( path ) );
			var before = history.Events.Fixed();
			sut.Run( Settings.Default );
			var items = history.Events.Select( item => item.MessageTemplate.Text ).Fixed();
			Assert.Contains( Resources.LoggerTemplates_NotFound, items );
			Assert.Contains( Resources.LoggerTemplates_Created, items );
			Assert.Equal( before.Length + 2, items.Length );
			Assert.True( File.Exists( path ) );

			Clear();
			Assert.False( File.Exists( path ) );
			sut.Run( Settings.Default );
			Assert.False( File.Exists( path ) );
			Assert.Equal( before.Length + 2, history.Events.Count() );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void CreateThenRecreate( InitializeUserSettingsCommand create, InitializeUserSettingsCommand sut, ILoggerHistory history )
		{
			create.Run( Settings.Default );
			var created = history.Events.Select( item => item.MessageTemplate.Text ).Fixed();
			Assert.Contains( Resources.LoggerTemplates_NotFound, created );
			Assert.Contains( Resources.LoggerTemplates_Created, created );

			var count = history.Events.Count();

			sut.Run( Settings.Default );
			Assert.Equal( count + 2, history.Events.Count() );
			var upgraded = history.Events.Select( item => item.MessageTemplate.Text ).Fixed();
			Assert.Contains( Resources.LoggerTemplates_Upgrading, upgraded );
			Assert.Contains( Resources.LoggerTemplates_Complete, upgraded );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void NoProperties( InitializeUserSettingsCommand create, InitializeUserSettingsCommand sut, ILoggerHistory history )
		{
			var before = history.Events.Fixed();
			sut.Run( new SettingsWithNoProperties() );
			var items = history.Events.Select( item => item.MessageTemplate.Text ).Fixed();
			Assert.Contains( Resources.LoggerTemplates_NotFound, items );
			Assert.Contains( Resources.LoggerTemplates_NotSaved, items );
			Assert.Equal( before.Length + 2, items.Length );
		}

		[Theory, DragonSpark.Testing.Framework.Setup.AutoData]
		public void Error( InitializeUserSettingsCommand create, InitializeUserSettingsCommand sut, ILoggerHistory history )
		{
			var before = history.Events.Fixed();
			sut.Run( new SettingsWithException() );
			var items = history.Events.Select( item => item.MessageTemplate.Text ).Fixed();
			Assert.Contains( Resources.LoggerTemplates_NotFound, items );
			Assert.Contains( Resources.LoggerTemplates_ErrorSaving, items );
			Assert.Equal( before.Length + 2, items.Length );
		}

		class SettingsWithNoProperties : ApplicationSettingsBase {}

		class SettingsWithException : ApplicationSettingsBase
		{
			[UserScopedSetting]
			public string HelloWorld
			{
				get { return (string)this[nameof(HelloWorld)]; }
				set { this[nameof(HelloWorld)] = value; }
			}

			public override void Save()
			{
				throw new ConfigurationErrorsException( "Some exception" );
			}
		}
	}
}