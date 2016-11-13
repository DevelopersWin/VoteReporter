using DragonSpark.Specifications;

namespace DragonSpark.Application
{
	public sealed class ApplicationPartsAssignedSpecification : SuppliedDelegatedSpecification<SystemParts?>
	{
		public static ISpecification<object> Default { get; } = new ApplicationPartsAssignedSpecification();
		ApplicationPartsAssignedSpecification() : base( AssignedSpecification<SystemParts?>.Default, ApplicationParts.Default.Get ) {}
	}
}