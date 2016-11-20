using DragonSpark.Sources;
using DragonSpark.Sources.Scopes;
using JetBrains.Annotations;

namespace DragonSpark.Specifications
{
	public class OncePerScopeSpecification<T> : ApplyConditionMonitorSpecificationBase<T>
	{
		public OncePerScopeSpecification() : this( new SingletonScope<ConditionMonitor>( () => new ConditionMonitor() ) ) {}

		[UsedImplicitly]
		public OncePerScopeSpecification( ISource<ConditionMonitor> source ) : base( source.Call ) {}
	}
}