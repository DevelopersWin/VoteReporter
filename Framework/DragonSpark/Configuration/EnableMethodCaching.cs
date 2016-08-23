using DragonSpark.Sources;

namespace DragonSpark.Configuration
{
	public class EnableMethodCaching : Scope<bool>
	{
		public static EnableMethodCaching Default { get; } = new EnableMethodCaching();
		EnableMethodCaching() : base( () => true ) {}
	}
}
