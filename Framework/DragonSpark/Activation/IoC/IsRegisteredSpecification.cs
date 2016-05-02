using DragonSpark.Runtime.Specifications;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Activation.IoC
{
	public class IsRegisteredSpecification : GuardedSpecificationBase<LocateTypeRequest>
	{
		readonly IUnityContainer container;

		public IsRegisteredSpecification( [Required]IUnityContainer container )
		{
			this.container = container;
		}

		public override bool IsSatisfiedBy( LocateTypeRequest parameter ) => container.IsRegistered( parameter.RequestedType, parameter.Name );
	}
}