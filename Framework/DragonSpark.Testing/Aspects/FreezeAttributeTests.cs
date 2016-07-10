using DragonSpark.Aspects;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects
{
	public class FreezeAttributeTests : TestCollectionBase
	{
		public FreezeAttributeTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void ProviderCached()
		{
			var sut = new Source();

			Assert.Equal( 1, sut.Cached );
			Assert.Equal( 1, sut.Cached );
		}

		/*[Fact]
		public void Performance()
		{
			new PerformanceSupport( WriteLine, Cached, Raw ).Run();
		}

		class IsFactorySpecification : DragonSpark.Activation.IsFactorySpecification
		{
			public new static IsFactorySpecification Instance { get; } = new IsFactorySpecification();

			IsFactorySpecification() : base( typeof(IFactory), typeof(IFactoryWithParameter) ) {}
		}

		[Fact]
		public void Raw()
		{
			IsFactorySpecification.Instance.IsSatisfiedBy( typeof(Factory) );
		}

		[Fact]
		public void Cached()
		{
			DragonSpark.Activation.IsFactorySpecification.Instance.Get( typeof(Factory) );
		}

		class Factory : FactoryBase<string, DateTime>
		{
			public override DateTime Create( string parameter ) => DateTime.Now;
		}*/

		[Fact]
		public void CheckFreeze()
		{
			var sut = new Disposable();
			sut.Dispose();
			Assert.Equal( 1, sut.Count );
			sut.Dispose();
			Assert.Equal( 1, sut.Count );

			sut.Other();
			Assert.Equal( 2, sut.Count );
			sut.Other();
			Assert.Equal( 2, sut.Count );
		}

		public class Disposable : IDisposable
		{
			public int Count { get; private set; }
			

			public void Dispose() => Dispose( true );

			public void Other() => Dispose( false );

			[Freeze]
			protected virtual void Dispose( bool disposing ) => Count++;
		}

		public class Source : AssemblyStoreBase
		{
			public int Count { get; private set; }

			[Freeze]
			public int Cached => ++Count;

			public Source() : base( Items<Assembly>.Default ) {}
		}

		[Theory, AutoData]
		public void BasicCache( CacheItem sut )
		{
			Assert.Equal( 1, sut.Count );
			sut.Up();
			Assert.Equal( 2, sut.Count );
			sut.Up();
			Assert.Equal( 2, sut.Count );

			sut.UpWith( 2 );
			Assert.Equal( 4, sut.Count );
			sut.UpWith( 2 );
			Assert.Equal( 4, sut.Count );
			sut.UpWith( 3 );
			Assert.Equal( 7, sut.Count );
			sut.UpWith( 3 );
			Assert.Equal( 7, sut.Count );
		}

		public class CacheItem
		{
			public int Count { get; private set; } = 1;

			[Freeze]
			public void Up()
			{
				Count++;
			}

			[Freeze]
			public void UpWith( int i )
			{
				Count += i;
			}
		}

		/*[Fact]
		[DotMemoryUnit( SavingStrategy = SavingStrategy.OnCheckFail, Directory = @"C:\dotMemory", CollectAllocations = true, FailIfRunWithoutSupport = false )]
		public void MemoryTest()
		{
			Test();

			dotMemory.Check( memory =>
							 {
								 Assert.Equal( 0, memory.GetObjects( where => where.Type.Is<MemoryTestFactory.Result>() ).ObjectsCount );
							 } );
		}

		static void Test()
		{
			var factory = new MemoryTestFactory();
			var parameter = new MemoryTestFactory.Parameter();
			var first = factory.Create( parameter );
			var second = factory.Create( parameter );
			Assert.Same( parameter, first.Parameter );
			Assert.Same( parameter, second.Parameter );
		}

		public class MemoryTestFactory : FactoryBase<MemoryTestFactory.Parameter, MemoryTestFactory.Result>
		{
			public class Parameter {}

			public class Result
			{
				public Result( Parameter parameter )
				{
					Parameter = parameter;
				}

				public Parameter Parameter { get; }
			}

			[Freeze]
			public override Result Create( Parameter parameter ) => new Result( parameter );
		}*/
	}
}