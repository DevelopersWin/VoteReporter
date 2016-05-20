using DragonSpark.Activation;
using DragonSpark.Aspects;
using Xunit;

namespace DragonSpark.Testing.Runtime
{
	public class ParameterWorkflowTests
	{
		[Fact]
		public void BasicCondition()
		{
			var sut = new Factory();
			var cannot = sut.CanCreate( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );

			var can = sut.CanCreate( 123 );
			Assert.True( can );
			Assert.Equal( 2, sut.CanCreateCalled );

			Assert.Equal( 0, sut.CreateCalled );

			var created = sut.Create( 123 );
			Assert.Equal( 2, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CreateCalled );
			Assert.Equal( 6776, created );
		}

		[Fact]
		public void ExtendedCheck()
		{
			for ( int i = 0; i < 10000; i++ )
			{
				var sut = new ExtendedFactory();
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );
			var cannot = sut.CanCreate( (object)456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );

			var can = sut.CanCreate( 6776 );
			Assert.True( can );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );

			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = sut.Create( (object)6776 );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
			}
		}

		[FactoryParameterValidator]
		public class Factory : IFactoryWithParameter
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return (int)parameter == 123;
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return 6776;
			}
		}

		[FactoryParameterValidator, GenericFactoryParameterValidator]
		class ExtendedFactory : IFactory<int, float>
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return parameter is int && CanCreate( (int)parameter );
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return Create( (int)parameter );
			}

			public bool CanCreate( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public float Create( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}
		}
	}
}
