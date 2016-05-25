using DragonSpark.Activation.IoC.Specifications;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using PostSharp.Patterns.Threading;

namespace DragonSpark.Activation.IoC
{
	[ThreadAffine]
	public class IsRegisteredSpecification : TypeRequestSpecificationBase<LocateTypeRequest>
	{
		readonly IUnityContainer container;

		public IsRegisteredSpecification( [Required]IUnityContainer container )
		{
			this.container = container;
		}

		public override bool IsSatisfiedBy( LocateTypeRequest parameter ) => container.IsRegistered( parameter.RequestedType, parameter.Name );
	}
}