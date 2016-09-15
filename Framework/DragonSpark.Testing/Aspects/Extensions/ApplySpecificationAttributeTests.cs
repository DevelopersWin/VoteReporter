using DragonSpark.Aspects.Extensions;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using Xunit;

namespace DragonSpark.Testing.Aspects.Extensions
{
	public class ApplySpecificationAttributeTests
	{
		[Fact]
		public void Verify()
		{
			var sut = new Subject();
			Assert.False( sut.IsSatisfiedBy( 123 ) );
			Assert.False( sut.Called );
			Assert.True( sut.IsSatisfiedBy( 6776 ) );
			Assert.False( sut.Called );
			Assert.False( sut.IsSatisfiedBy( 123 ) );
			Assert.False( sut.Called );
		}

		[ApplySpecification( typeof(Specification) )]
		class Subject : ISpecification<int>
		{
			public bool Called { get; private set; }

			public bool IsSatisfiedBy( int parameter )
			{
				Called = true;
				return false;
			}
		}

		sealed class Specification : SpecificationBase<int>
		{
			[UsedImplicitly]
			public static Specification Default { get; } = new Specification();
			Specification() {}

			public override bool IsSatisfiedBy( int parameter ) => parameter == 6776;
		}
	}
}