using DragonSpark.Sources;

namespace DragonSpark.Runtime.Application
{
	public sealed class CurrentTimeConfiguration : Scope<ICurrentTime>
	{
		public static CurrentTimeConfiguration Default { get; } = new CurrentTimeConfiguration();
		CurrentTimeConfiguration() : base( () => CurrentTime.Default ) {}
	}
}