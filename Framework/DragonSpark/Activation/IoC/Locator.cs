using DragonSpark.Extensions;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System.Linq;
using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Activation.IoC
{
	public class Locator : LocatorBase
	{
		readonly IUnityContainer container;
		
		public Locator( [Required]IUnityContainer container, IHandleTypeRequestSpecification support ) : base( new DecoratedSpecification<LocateTypeRequest>( support, Coercer.Instance ) )
		{
			this.container = container;
		}

		protected override object CreateItem( LocateTypeRequest parameter ) => container.TryResolve( parameter.RequestedType, parameter.Name );
	}

	public class Constructor : ConstructorBase
	{
		readonly IUnityContainer container;
		
		public Constructor( [Required]IUnityContainer container, [Required]IHandleTypeRequestSpecification support ) : base( new DecoratedSpecification<ConstructTypeRequest>( support, Coercer.Instance ) )
		{
			this.container = container;
		}

		protected override object CreateItem( ConstructTypeRequest parameter )
		{
			using ( var child = container.CreateChildContainer().Extend<DefaultRegistrationsExtension>() )
			{
				var command = child.Resolve<RegisterEntireHierarchyCommand>();
				parameter.Arguments.NotNull().Select( o => new InstanceRegistrationParameter( o ) ).Each( command.Run );

				var result = child.TryResolve( parameter.RequestedType );
				return result;
			}
		}
	}
}