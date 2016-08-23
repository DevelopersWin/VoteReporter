using DragonSpark.Activation.Location;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Testing.Objects;
using Xunit;

namespace DragonSpark.Testing.Activation.FactoryModel
{
	public class FactoryReflectionSupportTests
	{
		[Fact]
		public void GetResultType()
		{
			var expected = typeof(FactoryOfYac);

			new AssignSystemPartsCommand( expected ).Run();

			var type = SourceTypes.Default.Get().Get( new LocateTypeRequest( typeof(YetAnotherClass) ) );
			Assert.Equal( expected, type );
		}

		[Fact]
		public void Property()
		{
			Assert.IsType<YetAnotherClass>( ClassProperty );
		}

		[Fact]
		public void PropertyFromApplicationTypes()
		{
			new AssignSystemPartsCommand( typeof(FactoryOfYac) ).Run();
			Assert.IsType<YetAnotherClass>( ClassPropertyFromApplicationTypes );
		}

		[Factory( typeof(FactoryOfYac) )]
		public IInterface ClassProperty { get; set; }

		[Factory]
		public YetAnotherClass ClassPropertyFromApplicationTypes { get; set; }
	}

	
}