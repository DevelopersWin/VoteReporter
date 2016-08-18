using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture.Xunit2;
using System;
using DragonSpark.Diagnostics.Exceptions;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class ApplicationExceptionFormatterTests
	{
		[Theory, Framework.Setup.AutoData]
		public void Format( [Frozen]AssemblyInformation information, ApplicationExceptionFormatter sut, Exception exception )
		{
			var message = sut.Format( exception );
			Assert.Contains( information.Title, message );
			Assert.Contains( information.Product, message );
			Assert.Contains( information.Version.ToString(), message );
			Assert.Contains( exception.GetType().ToString(), message );
			Assert.Contains( exception.Message, message );
		} 
	}
}