using DragonSpark.Configuration;
using System.Composition.Convention;

namespace DragonSpark.Composition
{
	public class ConventionBuilderFactory : ConfigurableFactoryBase<ConventionBuilder>
	{
		public static ConventionBuilderFactory Default { get; } = new ConventionBuilderFactory();
		ConventionBuilderFactory() : base( () => new ConventionBuilder(), ConventionTransformer.Default ) {}
	}
}