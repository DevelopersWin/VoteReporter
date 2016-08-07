using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Testing.Objects;
using Xunit;

namespace DragonSpark.Testing.Activation.FactoryModel
{
	public class FactoryReflectionSupportTests
	{
		[Fact]
		public void GetResultType()
		{
			var expected = typeof(FactoryOfYAC);

			new AssignSystemPartsCommand( expected ).Run();

			var type = SourceTypes.Instance.Get().Get( new LocateTypeRequest( typeof(YetAnotherClass) ) );
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
			new AssignSystemPartsCommand( typeof(FactoryOfYAC) ).Run();
			Assert.IsType<YetAnotherClass>( ClassPropertyFromApplicationTypes );
		}

		[Factory( typeof(FactoryOfYAC) )]
		public IInterface ClassProperty { get; set; }

		[Factory]
		public YetAnotherClass ClassPropertyFromApplicationTypes { get; set; }
	}

	
}