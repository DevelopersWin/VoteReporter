namespace DragonSpark.ComponentModel
{
	public sealed class CurrentTimeAttribute : DefaultValueBase
	{
		public CurrentTimeAttribute() : base( t => new CurrentTimeValueProvider() ) {}
	}
}