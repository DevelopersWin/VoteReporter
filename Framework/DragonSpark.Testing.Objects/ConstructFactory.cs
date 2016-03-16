using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;

namespace DragonSpark.Testing.Objects
{
	class ConstructFactory : ConstructFactory<ClassWithParameter>
	{
		public ConstructFactory( IActivator activator ) : base( activator ) {}
	}
}