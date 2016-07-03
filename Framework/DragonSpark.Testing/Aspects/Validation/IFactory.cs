using DragonSpark.Activation;

namespace DragonSpark.Testing.Aspects.Validation
{
	public interface IFactory : IFactoryWithParameter
	{
		int CanCreateCalled { get; }

		int CreateCalled { get; }

		void Reset();
	}
}