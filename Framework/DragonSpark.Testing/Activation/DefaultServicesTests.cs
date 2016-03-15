using System.Reflection;
using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Objects.Setup;
using DragonSpark.TypeSystem;
using Xunit;
using Xunit.Abstractions;
using ServiceLocation = DragonSpark.Activation.ServiceLocation;

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
			var assemblies = Default<Assembly>.Items;
			var serviceLocator = DefaultServiceLocatorFactory.Instance.Create( new ServiceLocatorFactory.Parameter( CompositionHostFactory.Instance.Create( assemblies ), assemblies ) ); // new ServiceLocator( UnityContainerFactory.Instance.Create(), new RecordingLoggerFactory().Create() );
			Assert.NotNull( serviceLocator );
			sut.Assign( serviceLocator );

			var isAvailable = sut.IsAvailable;
			Assert.True( isAvailable );

			sut.Assign( null );
			Assert.False( sut.IsAvailable );
		}
	}
}