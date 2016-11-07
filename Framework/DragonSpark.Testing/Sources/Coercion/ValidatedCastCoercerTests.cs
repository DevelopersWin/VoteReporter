using DragonSpark.Diagnostics;
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
			Assert.Throws<InvalidOperationException>( () => ValidatedCastCoercer<LoggerConfiguration, LoggerFactory.LoggerConfiguration>.Default.Get( new LoggerConfiguration() ) );
			Assert.Throws<InvalidOperationException>( () => LoggerFactory.Factory.Implementation.Get( new LoggerConfiguration() ) );
		}
	}
}