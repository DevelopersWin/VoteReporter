namespace DragonSpark.Testing.Runtime.Properties
{
	public class EqualityCacheTests
	{
		/*class EqualityReference<T>
		{
			
		}*/

		/*[Theory, AutoData]
		public void EqualityCheck( EqualityReference<NamedTypeBuildKey> sut )
		{
			var one = NamedTypeBuildKey.Make<Class>();
			var two = NamedTypeBuildKey.Make<Class>();

			Assert.NotSame( one, two );

			Assert.Same( one, sut.Create( one ) );
			Assert.Same( one, sut.Create( two ) );
		}*/

		/*[Fact]
		public void HashCodeCheck()
		{
			var one = new TypeAdapter.MethodDescriptor( "SomeMethod", new[] { typeof(int), typeof(bool) }, 123, false );
			var two = new TypeAdapter.MethodDescriptor( "SomeMethod", new[] { typeof(int), typeof(bool) }, 123, false );

			Assert.Equal( one, two );
			Assert.True( one.Equals( two ) );
		}*/

		/*[Theory, AutoData]
		public void EqualityCheck( string name, int[] numbers, DateTime[] dates )
		{
			var one = new object[] { name }.Concat( numbers.Cast<object>() ).Concat( dates.Cast<object>() ).ToArray();
			var two = new object[] { name }.Concat( numbers.Cast<object>() ).Concat( dates.Cast<object>() ).ToArray();

			var equal = StructuralComparisons.StructuralEqualityComparer.Equals( one, two );
			Assert.True( equal );
		}


		[Theory, AutoData]
		public void DeepEqualityCheck( string name, int[] numbers, IEnumerable<DateTime> dates )
		{
			var one = new object[] { name, numbers, dates };
			var two = new object[] { name, numbers, dates };

			var equal = StructuralComparisons.StructuralEqualityComparer.Equals( one, two );
			Assert.True( equal );

			new Dictionary<object[], object>( StructuralComparisons.StructuralEqualityComparer )
		}*/

		/*struct Key
		{
			public Key( IEnumerable<struct> items ) {}
		}*/
	}
}