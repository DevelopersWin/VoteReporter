using DragonSpark.Sources;
using JetBrains.Annotations;

namespace DragonSpark.Specifications
{
	public sealed class OnlyOnceSpecification : OnlyOnceSpecification<object> {}

	public class OnlyOnceSpecification<T> : ApplyConditionMonitorSpecificationBase<T>
	{
		public OnlyOnceSpecification() : this( new ConditionMonitor() ) {}

		[UsedImplicitly]
		public OnlyOnceSpecification( ConditionMonitor monitor ) : base( monitor.Accept ) {} 
	}
}