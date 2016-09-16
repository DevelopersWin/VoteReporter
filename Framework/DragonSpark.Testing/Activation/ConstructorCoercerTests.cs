using DragonSpark.Activation;
using DragonSpark.Testing.Objects;
using Ploeh.AutoFixture.Xunit2;
using System;
using Xunit;

namespace DragonSpark.Testing.Activation
{
	public class ConstructorCoercerTests
	{
		[Theory, AutoData]
		public void Construct( ConstructorCoercer sut )
		{
			var parameter = sut.Coerce( typeof(Class) );
			Assert.Equal( parameter.RequestedType, typeof(Class) );
		} 

		[Theory, AutoData]
		void Parameter( ConstructCoercer<IntegerParameter> sut, int item )
		{
			var parameter = sut.Coerce( item );
			Assert.NotNull( parameter );
			Assert.Equal( parameter.SomeInteger, item );
			
		}

		[Theory, AutoData]
		void ConstructParameter( ConstructorCoercer sut, Type item )
		{
			var parameter = sut.Coerce( item );
			Assert.NotNull( parameter );
			Assert.Equal( parameter.RequestedType, item );
		}

		/*[Theory, AutoData]
		public void Fixed( [Frozen]Guid guid, [Greedy]FixedCoercer<Guid> sut, object parameter )
		{
			var result = sut.Coerce( parameter );
			Assert.Equal( guid, result );
		}*/

		class IntegerParameter
		{
			public IntegerParameter( int someInteger )
			{
				SomeInteger = someInteger;
			}

			public int SomeInteger { get; }
		}
	}
}