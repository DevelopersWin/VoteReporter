using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Specifications
{
	public sealed class Source : ConstructedSourceBase<ISpecificationAdapter>
	{
		public static Source Default { get; } = new Source();
		Source() : base( Constructor.Default.Get ) {}
	}
}