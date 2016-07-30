using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class FormatterFactoryTests
	{
		[Theory, AutoData, AdditionalTypes( typeof(MethodFormatter) ), FrameworkTypes]
		public void MethodFormatsAsExpected( [Service]FormatterFactory sut )
		{
			var method = MethodBase.GetCurrentMethod();
			var formatted = sut.Create( new FormatterFactory.Parameter( method ) );
			Assert.IsType<string>( formatted );
			Assert.Equal( new MethodFormatter( method ).ToString( null, null ), formatted );
		}
	}
}