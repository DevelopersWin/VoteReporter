using System.Reflection;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Application;
using DragonSpark.Testing.Framework.Application.Setup;
using DragonSpark.TypeSystem;
using Xunit;

namespace DragonSpark.Testing
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