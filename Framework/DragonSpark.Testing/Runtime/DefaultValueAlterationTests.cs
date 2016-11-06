using DragonSpark.Runtime;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using Xunit;

namespace DragonSpark.Testing.Runtime
{
	public class DefaultValueAlterationTests
	{
		[Fact]
		public void Verify()
		{
			var result = DefaultValueAlteration<IList<int>>.Default.Get();
			Assert.Same( Items<int>.Default, result );
		}
	}
}