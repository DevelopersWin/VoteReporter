using Ploeh.AutoFixture.Xunit2;
using System.ComponentModel;
using Xunit;

namespace DragonSpark.Testing.Aspects
{
	public class ApplyDefaultValuesTests
	{
		public class ValueHost
		{
			[DefaultValue( true )]
			public bool PropertyName { get; set; }

			[DefaultValue( 6776 )]
			public int Number { get; set; }
		}

		static class StaticValueHost
		{
			[DefaultValue( true )]
			public static bool PropertyName { get; set; }
		}

		[Fact]
		public void BasicAccess()
		{
			var sut = new ValueHost();
			var first = sut.PropertyName;
			Assert.True( first );

			Assert.Equal( 6776, sut.Number );

			var second = sut.PropertyName;
			Assert.True( second );

			var next = new ValueHost();
			Assert.True( next.PropertyName );

			/*var property = typeof(StaticValueHost).GetProperty( nameof(StaticValueHost.PropertyName) );
			var satisfied = DefaultValuePropertySpecification.Instance.IsSatisfiedBy( property );
			Assert.True( satisfied );*/

			var name = StaticValueHost.PropertyName;
			Assert.True( name );
		}

		[Theory, AutoData]
		public void Number( [NoAutoProperties]ValueHost sut, int number )
		{
			Assert.Equal( 6776, sut.Number );

			sut.Number = number;
			Assert.Equal( number, sut.Number );
		}
	}
}