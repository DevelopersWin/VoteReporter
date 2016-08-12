using DragonSpark.Activation;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Testing.Aspects.Validation
{
	public interface IFactory : IFactoryWithParameter
	{
		int CanCreateCalled { get; }

		int CreateCalled { get; }

		void Reset();
	}
}