using Ploeh.AutoFixture.Kernel;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	public class OptionalParameterTransformer : EnginePartFactory<Ploeh.AutoFixture.Kernel.ParameterRequestRelay>
	{
		public static OptionalParameterTransformer Default { get; } = new OptionalParameterTransformer();

		public override ISpecimenBuilder Get( Ploeh.AutoFixture.Kernel.ParameterRequestRelay parameter ) => new ParameterRequestRelay( parameter );
	}
}