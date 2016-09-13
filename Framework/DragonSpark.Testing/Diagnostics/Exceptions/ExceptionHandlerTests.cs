using System;
using DragonSpark.Diagnostics.Exceptions;
using Xunit;

namespace DragonSpark.Testing.Diagnostics.Exceptions
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