using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Windows.Testing.Setup
{
	public class StressTestingTests : TestCollectionBase
	{
		public StressTestingTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		
		public void GetAllTypesWith()
		{
			/*var result = Parallel.For( 0, 100000, i =>
									{
										

										
									} );
			Assert.True( result.IsCompleted );*/

			Body();

		}

		void Body()
		{
			var method = GetType().GetMethod( nameof( Host ) );
			var autoData = new AutoData( FixtureFactory<AutoDataCustomization>.Instance.Create(), method );
			var factory = Providers.From( data => new ServiceProviderFactory( new Composition.ServiceProviderFactory( AssemblyProvider.Instance.Create() ).Create ).Create(), serviceProvider => new Application<LocationSetup>( serviceProvider ) );
			using ( factory( autoData ) )
			{
				var data = new Ploeh.AutoFixture.Xunit2.AutoDataAttribute( autoData.Fixture ).GetData( method );

				var sut = data.Only().FirstOrDefaultOfType<Assembly[]>();
				var items = sut.GetAllTypesWith<PriorityAttribute>();
				Assert.True( items.Select( tuple => tuple.Item2 ).Contains( typeof(NormalPriority) ) );
			}
		}

		public void Host( [DragonSpark.Testing.Framework.Parameters.Service] Assembly[] sut ) {}

		/*class Cache : CacheFactoryBase
		{
			public Cache( Assembly[] assemblies ) : base( data => assemblies,  ) {}
		}*/
	}
}