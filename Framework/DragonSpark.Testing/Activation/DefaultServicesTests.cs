using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using Xunit;
using Xunit.Abstractions;
using AssemblyProvider = DragonSpark.Testing.Framework.Setup.AssemblyProvider;
using ServiceLocation = DragonSpark.Activation.ServiceLocation;
using ServiceLocatorFactory = DragonSpark.Setup.ServiceLocatorFactory;

namespace DragonSpark.Testing.Activation
{
	public class DefaultServicesTests : Tests
	{
		public DefaultServicesTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void IsAvailable()
		{
			var sut = Services.Location;

			Assert.Same( sut, ServiceLocation.Instance );

			Assert.False( sut.IsAvailable );
			var assemblies = AssemblyProvider.Instance.Create();
			var serviceLocator = ServiceLocatorFactory.Instance.Create( new ServiceProviderParameter( CompositionHostFactory.Instance.Create( assemblies ), assemblies ) ); // new ServiceLocator( UnityContainerFactory.Instance.Create(), new RecordingLoggerFactory().Create() );
			Assert.NotNull( serviceLocator );
			sut.Assign( serviceLocator );

			var isAvailable = sut.IsAvailable;
			Assert.True( isAvailable );

			sut.Assign( null );
			Assert.False( sut.IsAvailable );
		}
	}
}