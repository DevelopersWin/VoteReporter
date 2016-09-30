using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Application;
using DragonSpark.Testing.Objects.Properties;
using DragonSpark.Windows.Setup;
using JetBrains.Annotations;
using System.Configuration;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Resources = DragonSpark.Windows.Properties.Resources;

namespace DragonSpark.Windows.Testing.Setup
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class InitializeUserSettingsCommandTests : TestCollectionBase
	{
		public InitializeUserSettingsCommandTests( ITestOutputHelper output ) : base( output )
		{
			Clear();
		}

		static void Clear() => ClearUserSettingCommand.Default.Execute( Settings.Default );

		[Theory, AutoData]
		public void Create( InitializeUserSettingsCommand sut, ILoggerHistory history, UserSettingsPathFactory factory )
		{
			var path = factory.Get( ConfigurationUserLevel.PerUserRoamingAndLocal );
			Assert.False( path.Exists );
			var before = history.Events.Fixed();
			sut.Execute( Settings.Default );
			var items = history.Events.Select( item => item.MessageTemplate.Text ).Fixed();
			Assert.Contains( Resources.LoggerTemplates_NotFound, items );
			Assert.Contains( Resources.LoggerTemplates_Created, items );
			Assert.Equal( before.Length + 2, items.Length );

			path.Refresh();
			Assert.True( path.Exists );

			Clear();

			path.Refresh();
			Assert.False( path.Exists );
		}

		[Theory, AutoData]
		public void CreateThenRecreate( InitializeUserSettingsCommand sut, ILoggerHistory history )
		{
			sut.Execute( Settings.Default );
			var created = history.Events.Select( item => item.MessageTemplate.Text ).Fixed();
			Assert.Contains( Resources.LoggerTemplates_NotFound, created );
			Assert.Contains( Resources.LoggerTemplates_Created, created );

			var count = history.Events.Count();

			sut.Execute( Settings.Default );

			Assert.Equal( count + 2, history.Events.Count() );
			var upgraded = history.Events.Select( item => item.MessageTemplate.Text ).Fixed();
			Assert.Contains( Resources.LoggerTemplates_Upgrading, upgraded );
			Assert.Contains( Resources.LoggerTemplates_Complete, upgraded );
		}

		[Theory, AutoData]
		public void NoProperties( InitializeUserSettingsCommand create, InitializeUserSettingsCommand sut, ILoggerHistory history )
		{
			var before = history.Events.Fixed();
			sut.Execute( new SettingsWithNoProperties() );
			var items = history.Events.Select( item => item.MessageTemplate.Text ).Fixed();
			Assert.Contains( Resources.LoggerTemplates_NotFound, items );
			Assert.Contains( Resources.LoggerTemplates_NotSaved, items );
			Assert.Equal( before.Length + 2, items.Length );
		}

		[Theory, AutoData]
		public void Error( InitializeUserSettingsCommand create, InitializeUserSettingsCommand sut, ILoggerHistory history )
		{
			var before = history.Events.Fixed();
			sut.Execute( new SettingsWithException() );
			var items = history.Events.Select( item => item.MessageTemplate.Text ).Fixed();
			Assert.Contains( Resources.LoggerTemplates_NotFound, items );
			Assert.Contains( Resources.LoggerTemplates_ErrorSaving, items );
			Assert.Equal( before.Length + 2, items.Length );
		}

		class SettingsWithNoProperties : ApplicationSettingsBase {}

		class SettingsWithException : ApplicationSettingsBase
		{
			[UserScopedSetting, UsedImplicitly]
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