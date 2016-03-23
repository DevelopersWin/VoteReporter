using DragonSpark.TypeSystem;
using System;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class MemberInfoAttributeProviderFactoryTests
	{
		public string PropertyName { get; set; }

		[Fact]
		public void AsExpected()
		{
			var propertyInfo = GetType().GetProperty( nameof(PropertyName) );
			var local = new Tuple<MemberInfo, bool>( propertyInfo, false );
			var all = new Tuple<MemberInfo, bool>( propertyInfo, true );
			
			var sut = MemberInfoAttributeProviderFactory.Instance;
			var firstLocal = sut.Create( local );
			var secondLocal = sut.Create( local );

			Assert.Same( firstLocal, secondLocal );

			var firstAll = sut.Create( all );
			var secondAll = sut.Create( all );

			Assert.Same( firstAll, secondAll );

			Assert.NotSame( firstLocal, firstAll );
		} 
	}
}