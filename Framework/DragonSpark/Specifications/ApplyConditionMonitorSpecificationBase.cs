using System;

namespace DragonSpark.Specifications
{
	public abstract class ApplyConditionMonitorSpecificationBase<T> : SpecificationBase<T>
	{
		readonly Func<T, ConditionMonitor> source;
		protected ApplyConditionMonitorSpecificationBase( Func<T, ConditionMonitor> source )
		{
			this.source = source;
		}

		public override bool IsSatisfiedBy( T parameter ) => source( parameter ).Apply();
	}
}