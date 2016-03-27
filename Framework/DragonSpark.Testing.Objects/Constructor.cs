using DragonSpark.Activation;

namespace DragonSpark.Testing.Objects
{
	public class Constructor : FactoryBase<ClassWithParameter>
	{
		protected override ClassWithParameter CreateItem() => new ClassWithParameter( this );
	}
}