using DragonSpark.Specifications;
using System.Deployment.Application;

namespace DragonSpark.Windows.Setup
{
	public sealed class IsDeployedSpecification : SpecificationBase<object>
	{
		public static IsDeployedSpecification Default { get; } = new IsDeployedSpecification();
		IsDeployedSpecification() {}

		public override bool IsSatisfiedBy( object parameter ) => ApplicationDeployment.IsNetworkDeployed;
	}
}