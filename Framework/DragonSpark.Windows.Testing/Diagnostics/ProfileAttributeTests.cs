using DragonSpark.Windows.Diagnostics;
using System.Threading;
using Xunit;

namespace DragonSpark.Windows.Testing.Diagnostics
{
	public class ProfileAttributeTests
	{
		[Fact]
		public void Profile()
		{
			var tester = new PerformanceTester();
			tester.Perform();
			// tester.Perform();
		}

		class PerformanceTester
		{
			[Profile]
			public void Perform() => Thread.Sleep( 1000 );
		}
	}
}
