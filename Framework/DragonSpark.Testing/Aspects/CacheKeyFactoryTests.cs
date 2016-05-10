using DragonSpark.Aspects;
using DragonSpark.Testing.Objects;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DragonSpark.Testing.Aspects
{
	public class CacheKeyFactoryTests
	{
		[Theory, AutoData]
		public void EnsureSame( KeyFactory sut )
		{
			var first = sut.CreateUsing( typeof(Class), typeof(CacheKeyFactoryTests).GetMethod( nameof(EnsureSame) ), true );
			var second = sut.CreateUsing( typeof(Class), typeof(CacheKeyFactoryTests).GetMethod( nameof(EnsureSame) ), true );
			Assert.Equal( first, second );
		}

		[Theory, AutoData]
		public void EnsureDifferent( KeyFactory sut )
		{
			var first = sut.CreateUsing( typeof(Class), typeof(CacheKeyFactoryTests).GetMethod( nameof(EnsureSame) ), false );
			var second = sut.CreateUsing( typeof(Class), typeof(CacheKeyFactoryTests).GetMethod( nameof(EnsureSame) ), true );
			Assert.NotEqual( first, second );
		}

		/*[Fact]
		public void MemoryTest()
		{
			CacheValueFactory.Instance.Flush();
			DisposableRepository.Instance.DisposeAll();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			var temp = PropertyConnector.Default;
		}*/

		/*[Fact]
		public void ArrayCopyMetrics()
		{
			var first = new object[] { 1, 2, 3, "3", DateTime.UtcNow };
			var second = new object[] { 4, 5, "6", 7, "8" };

			for ( int i = 0; i < 100000; i++ )
			{
				Copy( first, second );
			}

			for ( int i = 0; i < 100000; i++ )
			{
				Resize( first, second );
			}
			
		}

		static void Resize( object[] first, object[] second )
		{
			var index = first.Length;
			Array.Resize( ref first, index + second.Length );
			Array.Copy( second, 0, first, index, second.Length );

		}

		static void Copy( object[] first, object[] second )
		{
			var items = new object[first.Length + second.Length];
			Array.Copy( first, items, first.Length );
			Array.Copy( second, 0, items, first.Length, second.Length );
		}*/
	}
}