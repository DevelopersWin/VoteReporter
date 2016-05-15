using DragonSpark.Activation;

namespace DragonSpark.Testing.Objects
{
	public class Constructor : FactoryBase<ClassWithParameter>
	{
		public override ClassWithParameter Create() => new ClassWithParameter( this );
	}
}