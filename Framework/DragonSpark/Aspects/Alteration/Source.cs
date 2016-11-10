using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Alteration
{
	public sealed class Source : ConstructedSourceBase<IAlterationAdapter>
	{
		public static Source Default { get; } = new Source();
		Source() : base( Constructor.Default.Get ) {}
	}
}