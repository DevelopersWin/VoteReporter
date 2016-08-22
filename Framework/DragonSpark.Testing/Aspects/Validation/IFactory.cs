using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Testing.Aspects.Validation
{
	public interface IFactory : IValidatedParameterizedSource
	{
		int CanCreateCalled { get; }

		int CreateCalled { get; }

		void Reset();
	}
}