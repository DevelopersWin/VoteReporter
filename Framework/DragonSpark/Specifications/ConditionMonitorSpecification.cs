namespace DragonSpark.Specifications
{
	public sealed class ConditionMonitorSpecification : DelegatedSpecification<ConditionMonitor>
	{
		public static ConditionMonitorSpecification Default { get; } = new ConditionMonitorSpecification();
		ConditionMonitorSpecification() : base( monitor => monitor.IsApplied ) {}
	}
}