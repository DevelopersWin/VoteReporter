using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Coercion
{
	public sealed class Source : ConstructedSourceBase<ICoercerAdapter>
	{
		public static Source Default { get; } = new Source();
		Source() : base( Constructor.Default.Get ) {}
	}
}