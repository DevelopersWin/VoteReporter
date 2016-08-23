using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation )]
	public class FormatterTests
	{
		[Theory, AutoData, FrameworkTypes]
		public void MethodFormatsAsExpected( [Service]Formatter sut )
		{
			var method = MethodBase.GetCurrentMethod();
			var formatted = sut.Get( new Formatter.Parameter( method ) );
			Assert.IsType<string>( formatted );
			Assert.Equal( new MethodFormatter( method ).ToString(), formatted );
		}
	}
}