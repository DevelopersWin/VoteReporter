using System;
using DragonSpark.Specifications;
using Xunit;

namespace DragonSpark.Testing.Runtime.Specifications
{
	public class SpecificationExtensionsTests
	{
		[Fact]
		public void And()
		{
			var sut = new SuppliedSpecification( true ).And( new SuppliedSpecification( true ) );
			Assert.True( sut.IsSatisfiedBy( null ) );
		} 

		[Fact]
		public void AndNot()
		{
			var sut = new SuppliedSpecification( true ).And( new SuppliedSpecification( false ) );
			Assert.False( sut.IsSatisfiedBy( null ) );
		}

		[Fact]
		public void Or()
		{
			var sut = new SuppliedSpecification( true ).Or( new SuppliedSpecification( false ) );
			Assert.True( sut.IsSatisfiedBy( null ) );
		}

		[Fact]
		public void Cached()
		{
			var count = 0;
			var specification = new DelegatedSpecification<Type>( type =>
																  {
																	  ++count;
																	  return true;
																  } );
			var cached = specification.ToCachedSpecification();
			Assert.NotSame( specification, cached );
			Assert.Same( cached, specification.ToCachedSpecification() );
			Assert.True( cached.IsSatisfiedBy( typeof(int) ) );
			Assert.Equal( 1, count );
			Assert.True( cached.IsSatisfiedBy( typeof(int) ) );
			Assert.Equal( 1, count );
			Assert.True( specification.IsSatisfiedBy( typeof(int) ) );
			Assert.Equal( 2, count );
			Assert.True( specification.IsSatisfiedBy( typeof(int) ) );
			Assert.Equal( 3, count );
			Assert.True( cached.IsSatisfiedBy( typeof(bool) ) );
			Assert.Equal( 4, count );
			Assert.True( cached.IsSatisfiedBy( typeof(bool) ) );
			Assert.Equal( 4, count );
		}
	}
}