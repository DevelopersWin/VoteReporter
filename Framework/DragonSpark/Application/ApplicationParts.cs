using DragonSpark.Sources;

namespace DragonSpark.Application
{
	public sealed class ApplicationParts : Scope<SystemParts>
	{
		public static IScope<SystemParts> Default { get; } = new ApplicationParts();
		ApplicationParts() : base( () => SystemParts.Default ) {}
	}
}