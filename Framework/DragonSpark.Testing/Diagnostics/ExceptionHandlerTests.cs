using DragonSpark.Diagnostics;
using System;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	public class ExceptionHandlerTests
	{
		[Theory, Ploeh.AutoFixture.Xunit2.AutoData]
		void Process( ExceptionHandler sut, Exception error )
		{
			Assert.Throws<Exception>( () => sut.Process( error ) );
		}
	}
}