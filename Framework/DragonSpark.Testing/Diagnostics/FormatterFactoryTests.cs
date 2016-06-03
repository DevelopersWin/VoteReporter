using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class FormatterFactoryTests
	{
		[Theory, Framework.Setup.AutoData( additionalTypes: typeof(MethodFormatter) )]
		public void MethodFormatsAsExpected( FormatterFactory sut )
		{
			var method = MethodBase.GetCurrentMethod();
			var formatted = sut.Create( new FormatterFactory.Parameter( method ) );
			Assert.IsType<string>( formatted );
			Assert.Equal( new MethodFormatter( method ).ToString( null, null ), formatted );
		}
	}
}