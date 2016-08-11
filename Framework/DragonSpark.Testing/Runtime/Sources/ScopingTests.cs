using DragonSpark.Runtime.Sources;
using Xunit;

namespace DragonSpark.Testing.Runtime.Sources
{
	public class ScopingTests
	{
		[Fact]
		public void CachingTests()
		{
			Assert.Same( Scope.Instance.Get(), Scope.Instance.Get() );
		}

		class Scope : Scope<object>
		{
			public static Scope Instance { get; } = new Scope();
			Scope() : base( Factory.Fix( () => new object() ) ) {}
		}
	}
}