using DragonSpark.Testing.Framework;
using System.Diagnostics;
using Xunit.Abstractions;

namespace DragonSpark.Windows.Testing.Setup
{
	public class StressTestingTests : TestCollectionBase
	{
		public StressTestingTests( ITestOutputHelper output ) : base( output ) {}

		/*[Fact]
		[DotMemoryUnit( SavingStrategy = SavingStrategy.OnCheckFail, Directory = @"C:\dotMemory", CollectAllocations = true, FailIfRunWithoutSupport = false )]
		[AssertTraffic( AllocatedObjectsCount = 0 )]
		public void GetAllTypesWith()
		{
			First();
			Second();
		}

		void Second() => First();

		void First()
		{
			var method = new Action<Assembly[]>( Host ).Method;
			var autoData = new AutoData( FixtureFactory<AutoDataCustomization>.Instance.Create(), method );
			var providerFactory = new Composition.TypeBasedServiceProviderFactory( GetType().ToItem() );
			var provider = new ServiceProviderFactory( providerFactory ).Create();
			var factory = new AutoDataExecutionContextFactory( provider.Wrap<AutoData, IServiceProvider>().ToDelegate(), LocationSetup.AutoDataAttribute.ApplicationSource );
			using ( factory.Create( autoData ) )
			{
				var data = new Ploeh.AutoFixture.Xunit2.AutoDataAttribute( autoData.Fixture ).GetData( method );

				var sut = data.Only().FirstOrDefaultOfType<Assembly[]>();
				Assert.NotNull( sut );
			}
		}

		void Host( [DragonSpark.Testing.Framework.Parameters.Service] Assembly[] sut ) {}*/

		/*[Fact]
		public void GetAllTypesWith2()
		{
			var sut = new[] { GetType(), typeof(NormalPriority), typeof(ServiceLocator), typeof(AutoDataAttribute), typeof(FileSystemAssemblySource) }.Select( type => type.Assembly ).ToArray();

			var mock = new Mock();
			var result = Parallel.For( 0, 10000, i =>
									{
										var items = sut.GetAllTypesWith<PriorityAttribute>();
										Assert.True( items.Select( tuple => tuple.Item2 ).Contains( typeof(NormalPriority) ) );

										Action action = mock.Hello;
										AssociatedContext.Default.Set( action.Method, new DisposableAction( () => {} ) );
										new ApplicationOutputCommand().Execute( new OutputCommand.Parameter( action ) );
										
									} );
			Assert.True( result.IsCompleted );
		}*/

		/*[Fact]
		public void Stress()
		{
			var result = Parallel.For( 0, 100000, i =>
									{
										/*var mock = new Mock();
										var methodInfo = typeof(Mock).GetMethod( nameof<>(Mock.Hello) );
										AssociatedContext.Property.Set( methodInfo, new DisposableAction( () => {} ) );
										new ApplicationOutputCommand().Run( new OutputCommand.Parameter( mock, methodInfo, mock.Hello ) );
										Framework.Setup.ExecutionContext.Instance.Verify(); // TODO: Remove.
										Framework.Setup.ExecutionContext.Instance.Value.Dispose();#1#
									} );
			Assert.True( result.IsCompleted );
		}*/

		class Mock
		{
			public void Hello()
			{
				Debug.WriteLine( "Hello World!" );
			}
		}
	}
}