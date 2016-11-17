using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using Serilog;
using System;
using Xunit;

namespace DragonSpark.Testing.Sources.Coercion
{
	public class ValidatedCastCoercerTests
	{
		[Fact]
		public void Verify()
		{
			Assert.Throws<InvalidOperationException>( () => ValidatedCastCoercer<LoggerConfiguration, ISourceAware>.Default.Get( new LoggerConfiguration() ) );
		}
	}
}