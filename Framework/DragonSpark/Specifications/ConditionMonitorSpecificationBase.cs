using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Specifications
{
	public abstract class ConditionMonitorSpecificationBase<T> : SpecificationBase<T>
	{
		readonly Func<T, ConditionMonitor> source;
		protected ConditionMonitorSpecificationBase( Func<T, ConditionMonitor> source )
		{
			this.source = source;
		}

		public override bool IsSatisfiedBy( [Optional]T parameter ) => source( parameter ).Apply();
	}
}