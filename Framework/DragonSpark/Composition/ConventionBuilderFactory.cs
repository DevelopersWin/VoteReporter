using DragonSpark.Extensions;
using DragonSpark.Sources.Scopes;
using System.Composition.Convention;

namespace DragonSpark.Composition
{
	public sealed class ConventionBuilderFactory : AggregateSource<ConventionBuilder>
	{
		public static ConventionBuilderFactory Default { get; } = new ConventionBuilderFactory();
		ConventionBuilderFactory() : base( ConventionConfiguration.Default.Fix() ) {}
	}
}