namespace DragonSpark.Testing.Setup.Registration
{
	// [ReaderWriterSynchronized]
	/*public class ApplicationFactory : Factory<MethodInfo, IApplication>
	{
		public static ApplicationFactory Instance { get; } = new ApplicationFactory();

		// [Writer]
		protected override IApplication CreateItem( MethodInfo parameter )
		{
			using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( parameter ) ) )
			{
				var autoData = new AutoData( FixtureFactory.Instance.Create(), parameter );
				var application = new Framework.Setup.ApplicationFactory( GetType().ToItem(), false ).Create( autoData );
				using ( new ExecuteApplicationCommand( application ).ExecuteWith( autoData ) )
				{
					autoData.Initialize();

					return application;

					/*var logger = application.Get<ILogger>();
					application.Get<LoggingLevelSwitch>().MinimumLevel = LogEventLevel.Debug;
					logger.Information( "Basic Logging: TestingCommandPerformance" );
					Testing( "Hello World!" );#1#
				}
				// new InitializeOutputCommand( Output ).Run( GetType() );
				/*application.Get<RecordingLogEventSink>().With( PurgingEventFactory.Instance.Create ).Each( Output.WriteLine );#1#
			}
		}

		/*[Export, Shared]
		public class RecordingLoggerFactory : DragonSpark.Diagnostics.RecordingLoggerFactory
		{
			[Export]
			public override LoggingLevelSwitch LevelSwitch => base.LevelSwitch;

			[Export]
			public override RecordingLogEventSink Sink => base.Sink;
		}#1#
	}*/

	/*public class MetadataRegistrationCommandOtherTests : Tests
	{
		public MetadataRegistrationCommandOtherTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void FirstOther()
		{
			var stopwatch = new Stopwatch().With( sw => sw.Start() );
			MethodBase.GetCurrentMethod().As<MethodInfo>( methodUnderTest =>
			{
				using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( methodUnderTest ) ) )
				{
					var autoData = new AutoData( FixtureFactory.Instance.Create(), methodUnderTest );
					var application = new Application();
					using ( new ExecuteApplicationCommand( application ).ExecuteWith( autoData ) )
					{
						autoData.Initialize();

						/*var registerFromMetadataCommand = application.Get<RegisterFromMetadataCommand>();
						registerFromMetadataCommand.ExecuteWith( new object() );#1#
						
						/*var customization = new CompositionCustomization();
						var item = customization.AutoData;#1#
						// Debugger.Break();
					}
				}

				/*ApplicationFactory.Instance.Create( methodUnderTest );#1#
			} );

			/*var info = typeof(CompositionCustomization).GetProperty( nameof(CompositionCustomization.AutoData) );

			var meets = DefaultValuePropertySpecification.Instance.IsSatisfiedBy( info );
			var asdf = DefaultValuePropertySpecification.Instance.IsSatisfiedBy( info );#1#
			Output.WriteLine( $"Complete: {stopwatch.ElapsedMilliseconds}." );
		}

		[Fact]
		public void SecondOther()
		{
			var stopwatch = new Stopwatch().With( sw => sw.Start() );
			MethodBase.GetCurrentMethod().As<MethodInfo>( methodUnderTest =>
			{
				ApplicationFactory.Instance.Create( methodUnderTest );
			} );
			Output.WriteLine( $"Complete: {stopwatch.ElapsedMilliseconds}." );
		}
	}*/
}
