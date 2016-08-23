using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Specifications
{
	public class OncePerScopeSpecification<T> : ConditionMonitorSpecificationBase<T>
	{
		public OncePerScopeSpecification() : this( new Scope<ConditionMonitor>( Factory.Global( () => new ConditionMonitor() ) ) ) {}

		public OncePerScopeSpecification( ISource<ConditionMonitor> source ) : base( source.Wrap<T, ConditionMonitor>() ) {}
	}
}