using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Windows.Testing.Setup;
using System.Diagnostics;
using System.Reflection;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture.Kernel;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Setup.Registration
{
	public class MetadataRegistrationCommandTests : Tests
	{
		public MetadataRegistrationCommandTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void First()
		{
			/*var first = Default<ISpecimenBuilder>.Items;
			var second = Default<ISpecimenBuilder>.Items;

			var third = Default<ISpecimenBuilder>.Item;
			var fourth = Default<ISpecimenBuilder>.Item;*/

			Create();
			Second();
			/*var stopwatch = new Stopwatch().With( sw => sw.Start() );
			MethodBase.GetCurrentMethod().As<MethodInfo>( methodUnderTest =>
			{
				ApplicationFactory.Instance.Create( methodUnderTest );
			} );
			Output.WriteLine( $"Complete: {stopwatch.ElapsedMilliseconds}." );*/
		}

		void Second()
		{
			Create();
		}

		void Create()
		{
			var stopwatch = new Stopwatch().With( sw => sw.Start() );
			MethodBase.GetCurrentMethod().As<MethodInfo>( methodUnderTest =>
			{
				using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( methodUnderTest ) ) )
				{
					var autoData = new AutoData( FixtureFactory.Instance.Create(), methodUnderTest );
					var application = new Windows.Testing.Setup.Application<LocationSetup>();
					using ( new ExecuteApplicationCommand( application ).ExecuteWith( autoData ) )
					{
						/*var first = GetType().Assembly.Has<RegistrationBaseAttribute>();
						var second = GetType().Assembly.Has<RegistrationBaseAttribute>();
						Debugger.Break();*/

						/*var logger1 = application.Get<ILogger>();
						logger1.With( logger => logger.Information( $"Initialized: {stopwatch.ElapsedMilliseconds}" ) );*/
						// autoData.Initialize();

						/*var registerFromMetadataCommand = application.Get<RegisterFromMetadataCommand>();
						registerFromMetadataCommand.ExecuteWith( new object() );*/

						/*var customization = new CompositionCustomization();
						var item = customization.AutoData;*/
						// Debugger.Break();
					}
				}

				/*ApplicationFactory.Instance.Create( methodUnderTest );*/
			} );

			/*var info = typeof(CompositionCustomization).GetProperty( nameof(CompositionCustomization.AutoData) );

			var meets = DefaultValuePropertySpecification.Instance.IsSatisfiedBy( info );
			var asdf = DefaultValuePropertySpecification.Instance.IsSatisfiedBy( info );*/
			Output.WriteLine( $"Complete: {stopwatch.ElapsedMilliseconds}." );
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