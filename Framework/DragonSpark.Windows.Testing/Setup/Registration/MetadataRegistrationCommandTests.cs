using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Windows.Testing.Setup;
using System.Diagnostics;
using System.Reflection;
using DragonSpark.Setup;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Setup.Registration
{
	public class MetadataRegistrationCommandTests : TestBase
	{
		public MetadataRegistrationCommandTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void First()
		{
			/*var first = Default<ISpecimenBuilder>.Items;
			var second = Default<ISpecimenBuilder>.Items;

			var third = Default<ISpecimenBuilder>.Item;
			var fourth = Default<ISpecimenBuilder>.Item;*/

			Output.WriteLine( "Basic:" );

			Basic();
			for ( int i = 0; i < 10; i++ )
			{
				Second();
			}

			Output.WriteLine( "Full:" );

			Full();

			for ( int i = 0; i < 10; i++ )
			{
				Third();
			}
			/*var stopwatch = new Stopwatch().With( sw => sw.Start() );
			MethodBase.GetCurrentMethod().As<MethodInfo>( methodUnderTest =>
			{
				ApplicationFactory.Instance.Create( methodUnderTest );
			} );
			Output.WriteLine( $"Complete: {stopwatch.ElapsedMilliseconds}." );*/
		}

		[Fact]
		public void SecondFact()
		{
			First();
		}

		void Second() => Basic();

		void Third() => Full();

		void Basic()
		{
			/*var stopwatch = new Stopwatch().With( sw => sw.Start() );
			MethodBase.GetCurrentMethod().As<MethodInfo>( methodUnderTest =>
			{
				using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( methodUnderTest ) ) )
				{
					var autoData = new AutoData( FixtureFactory.Instance.Create(), methodUnderTest );
					using ( var application = new Framework.Setup.ApplicationFactory( GetType().ToItem(), false ).Create( autoData ) )
					{
						using ( new ExecuteApplicationCommand( application ).ExecuteWith( autoData ) )
						{
							/*var first = GetType().Assembly.Has<RegistrationBaseAttribute>();
							var second = GetType().Assembly.Has<RegistrationBaseAttribute>();
							Debugger.Break();#1#

							/*var logger1 = application.Get<ILogger>();
							logger1.With( logger => logger.Information( $"Initialized: {stopwatch.ElapsedMilliseconds}" ) );#1#
							// autoData.Initialize();

							/*var registerFromMetadataCommand = application.Get<RegisterFromMetadataCommand>();
							registerFromMetadataCommand.ExecuteWith( new object() );#1#

							/*var customization = new CompositionCustomization();
							var item = customization.AutoData;#1#
							// Debugger.Break();
						}
					}
				}

				/*ApplicationFactory.Instance.Create( methodUnderTest );#1#
			} );*/

			/*var info = typeof(CompositionCustomization).GetProperty( nameof(CompositionCustomization.AutoData) );

			var meets = DefaultValuePropertySpecification.Instance.IsSatisfiedBy( info );
			var asdf = DefaultValuePropertySpecification.Instance.IsSatisfiedBy( info );*/
			// Output.WriteLine( $"Complete: {stopwatch.ElapsedMilliseconds}." );
		}

		void Full()
		{
			/*var stopwatch = new Stopwatch().With( sw => sw.Start() );
			MethodBase.GetCurrentMethod().As<MethodInfo>( methodUnderTest =>
			{
				using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( methodUnderTest ) ) )
				{
					var autoData = new AutoData( FixtureFactory.Instance.Create(), methodUnderTest );
					using ( var application = new Windows.Testing.Setup.Application<LocationSetup>() )
					{
						using ( new ExecuteApplicationCommand( application ).ExecuteWith( autoData ) )
						{
							/*var first = GetType().Assembly.Has<RegistrationBaseAttribute>();
							var second = GetType().Assembly.Has<RegistrationBaseAttribute>();
							Debugger.Break();#1#

							/*var logger1 = application.Get<ILogger>();
							logger1.With( logger => logger.Information( $"Initialized: {stopwatch.ElapsedMilliseconds}" ) );#1#
							// autoData.Initialize();

							/*var registerFromMetadataCommand = application.Get<RegisterFromMetadataCommand>();
							registerFromMetadataCommand.ExecuteWith( new object() );#1#

							/*var customization = new CompositionCustomization();
							var item = customization.AutoData;#1#
							// Debugger.Break();
						}
					}
				}

				/*ApplicationFactory.Instance.Create( methodUnderTest );#1#
			} );

			/*var info = typeof(CompositionCustomization).GetProperty( nameof(CompositionCustomization.AutoData) );

			var meets = DefaultValuePropertySpecification.Instance.IsSatisfiedBy( info );
			var asdf = DefaultValuePropertySpecification.Instance.IsSatisfiedBy( info );#1#
			Output.WriteLine( $"Complete: {stopwatch.ElapsedMilliseconds}." );*/
		}

		/*[Fact]
		public void Second() 
		{
			var stopwatch = new Stopwatch().With( sw => sw.Start() );
			MethodBase.GetCurrentMethod().As<MethodInfo>( methodUnderTest =>
			{
				ApplicationFactory.Instance.Create( methodUnderTest );
			} );
			Output.WriteLine( $"Complete: {stopwatch.ElapsedMilliseconds}." );
		}*/
	}
}