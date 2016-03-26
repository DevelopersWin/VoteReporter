using DragonSpark.Runtime.Specifications;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Activation.IoC
{
	public class IsRegisteredSpecification : SpecificationBase<LocateTypeRequest>
	{
		readonly IUnityContainer container;

		public IsRegisteredSpecification( [Required]IUnityContainer container )
		{
			this.container = container;
		}

		protected override bool Verify( LocateTypeRequest parameter ) => container.IsRegistered( parameter.RequestedType, parameter.Name );
	}
}