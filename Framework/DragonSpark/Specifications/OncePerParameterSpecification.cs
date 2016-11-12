using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using JetBrains.Annotations;

namespace DragonSpark.Specifications
{
	public class OncePerParameterSpecification<T> : ConditionMonitorSpecificationBase<T> where T : class
	{
		public OncePerParameterSpecification() : this( new Condition<T>() ) {}

		[UsedImplicitly]
		public OncePerParameterSpecification( IParameterizedSource<T, ConditionMonitor> cache ) : base( cache.ToDelegate() ) {}
	}
}