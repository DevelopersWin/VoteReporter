using DragonSpark.Activation;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using System.Composition.Hosting;
using Xunit;
using Xunit.Abstractions;
using ServiceLocation = DragonSpark.Activation.ServiceLocation;

namespace DragonSpark.Testing.Activation
{
	public class DefaultServicesTests : Tests
	{
		public DefaultServicesTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, AutoData]
		public void IsAvailable( ServiceLocatorFactory.Parameter parameter  )
		{
			var sut = Services.Location;

			Assert.Same( sut, ServiceLocation.Instance );

			Assert.False( sut.IsAvailable );

			var serviceLocator = ServiceLocatorFactory.Configured.Create( parameter ); // new ServiceLocator( UnityContainerFactory.Instance.Create(), new RecordingLoggerFactory().Create() );
			Assert.NotNull( serviceLocator );
			sut.Assign( serviceLocator );

			var isAvailable = sut.IsAvailable;
			Assert.True( isAvailable );

			sut.Assign( null );
			Assert.False( sut.IsAvailable );
		}
	}
}