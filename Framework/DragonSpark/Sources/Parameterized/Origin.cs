using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Sources.Parameterized
{
	public sealed class Origin : Cache<ISource>
	{
		public static IAssignableParameterizedSource<ISource> Default { get; } = new Origin();
		Origin() {}
	}
}