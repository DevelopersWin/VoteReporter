using DragonSpark.Activation.Location;
using DragonSpark.Application;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Testing.Objects;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Activation.FactoryModel
{
	public class FactoryReflectionSupportTests
	{
		[Fact]
		public void GetResultType()
		{
			var expected = typeof(FactoryOfYac).Yield().AsApplicationParts();
			var type = SourceTypes.Default.Get().Get( new LocateTypeRequest( typeof(YetAnotherClass) ) );
			Assert.Equal( expected.Single(), type );
		}

		[Fact]
		public void Property()
		{
			Assert.IsType<YetAnotherClass>( ClassProperty );
		}

		[Fact]
		public void PropertyFromApplicationTypes()
		{
			ApplicationParts.Assign( typeof(FactoryOfYac) );
			Assert.IsType<YetAnotherClass>( ClassPropertyFromApplicationTypes );
		}

		[Factory( typeof(FactoryOfYac) )]
		public IInterface ClassProperty { get; set; }

		[Factory]
		public YetAnotherClass ClassPropertyFromApplicationTypes { get; set; }
	}

	
}