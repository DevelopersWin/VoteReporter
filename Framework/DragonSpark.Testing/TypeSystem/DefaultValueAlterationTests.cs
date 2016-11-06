using DragonSpark.TypeSystem;
using System.Collections.Generic;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class DefaultValueAlterationTests
	{
		[Fact]
		public void Verify()
		{
			IList<int> instance = null;
			var result = DefaultValueAlteration<IList<int>>.Default.Get( instance );
			Assert.Same( Items<int>.Default, result );
		}
	}
}