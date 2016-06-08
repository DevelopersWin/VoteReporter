using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using JetBrains.dotMemoryUnit;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Windows.Testing.Setup
{
	public class StressTestingTests : TestCollectionBase
	{
		public StressTestingTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		[DotMemoryUnit( SavingStrategy = SavingStrategy.OnCheckFail, Directory = @"C:\dotMemory", CollectAllocations = true, FailIfRunWithoutSupport = false )]
		[AssertTraffic( AllocatedObjectsCount = 0 )]
		public void GetAllTypesWith()
		{
			var method = GetType().GetMethod( nameof(Host) );
			var autoData = new AutoData( FixtureFactory<AutoDataCustomization>.Instance.Create(), method );
			var factory = Providers.From( data => new ServiceProviderFactory( new Composition.ServiceProviderFactory( GetType().ToItem() ).Create ).Create(), serviceProvider => new Application<LocationSetup>( serviceProvider ) );
			using ( factory( autoData ) )
			{
				var data = new Ploeh.AutoFixture.Xunit2.AutoDataAttribute( autoData.Fixture ).GetData( method );

				var sut = data.Only().FirstOrDefaultOfType<Assembly[]>();
				Assert.NotNull( sut );
				/*
				var items = sut.GetAllTypesWith<PriorityAttribute>();
				Assert.True( items.Select( tuple => tuple.Item2 ).Contains( typeof(NormalPriority) ) );*/
			}

			/*var result = Parallel.For( 0, 100000, i =>
									{
										

										
									} );
			Assert.True( result.IsCompleted );*/
			// var snapshot = dotMemory.Check();
			/*Body();

			dotMemory.Check( memory =>
							 {
								 var temp = memory.GetDifference( snapshot ).GetSurvivedObjects().ObjectsCount;
								 Assert.Equal( 0, temp );
							 } );*/
		}
		
		public void Host( [DragonSpark.Testing.Framework.Parameters.Service] Assembly[] sut ) {}


		/*[Fact]
		public void Properties()
		{
			var current = DateTime.Now;
			var target = new ClassWithDefaultProperties();

			Assert.Equal( 'd', target.Char );
			Assert.Equal( 7, target.Byte );
			Assert.Equal( 8, target.Short );
			Assert.Equal( 9, target.Int );
			Assert.Equal( 6776, target.Long );
			Assert.Equal( 6.7F, target.Float );
			Assert.Equal( 7.1, target.Double );
			Assert.True( target.Boolean );
			Assert.Equal( "Hello World", target.String );
			Assert.Equal( "Legacy", target.Legacy );
			
			Assert.Equal( typeof(ClassWithDefaultProperties), target.Object );

			Assert.NotEqual( DateTime.MinValue, target.CurrentDateTime );
			Assert.NotEqual( DateTimeOffset.MinValue, target.CurrentDateTimeOffset );

			Assert.True( target.CurrentDateTime >= current );
			Assert.True( target.CurrentDateTimeOffset >= current );

			Assert.NotNull( target.Activated );

			var created = Assert.IsType<ClassWithParameter>( target.Factory );
			Assert.NotNull( created.Parameter );
			Assert.IsType<DragonSpark.Testing.Objects.Constructor>( created.Parameter );

			Assert.NotNull( target.Collection );
			Assert.IsAssignableFrom<System.Collections.ObjectModel.Collection<object>>( target.Collection );
			Assert.NotNull( target.Classes );
			Assert.IsAssignableFrom<System.Collections.ObjectModel.Collection<Class>>( target.Classes );

			Assert.Equal( 6776, target.ValuedInt );

			Assert.NotEqual( Guid.Empty, target.Guid );
			Assert.NotEqual( Guid.Empty, target.AnotherGuid );

			Assert.NotEqual( target.Guid, target.AnotherGuid );

			Assert.Equal( new Guid( "66570344-BA99-4C90-A7BE-AEC903441F97" ), target.ProvidedGuid );

			Assert.Equal( "Already Set", target.AlreadySet );
		}*/
		/*class Cache : CacheFactoryBase
		{
			public Cache( Assembly[] assemblies ) : base( data => assemblies,  ) {}
		}*/
	}
}