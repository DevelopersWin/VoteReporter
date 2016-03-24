using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects
{
	public class KeyFactoryTests : Tests
	{
		public KeyFactoryTests( ITestOutputHelper output ) : base( output ) {}

		[Fact]
		public void Basic()
		{
			using ( var tracer = new TracerFactory( Output.WriteLine ).Create() )
			{
				// tracer.Diagnostics.Logger.Information( "Hello World!" );
			}
		}
	}
}