using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.TypeSystem
{
	public class ExtensionsTests
	{
		[Fact]
		public void IsInstanceOfType()
		{
			
			Assert.Contains( GetType(), this.Yield().AsTypes() );
			Assert.Contains( GetType(), this.Yield().AsEnumerable().SelectTypes() );
			
		}
	}
}