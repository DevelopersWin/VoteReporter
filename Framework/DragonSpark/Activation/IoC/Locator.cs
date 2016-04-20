using DragonSpark.Extensions;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System.Linq;

namespace DragonSpark.Activation.IoC
{
	class Locator : LocatorBase
	{
		readonly IUnityContainer container;
		
		public Locator( [Required]IUnityContainer container, IResolutionSupport support ) : base( support )
		{
			this.container = container;
		}

		protected override object CreateItem( LocateTypeRequest parameter ) => container.TryResolve( parameter.RequestedType, parameter.Name );
	}

	class Constructor : ConstructorBase
	{
		readonly IUnityContainer container;
		
		public Constructor( [Required]IUnityContainer container, [Required]IResolutionSupport support ) : base( support )
		{
			this.container = container;
		}

		protected override object CreateItem( ConstructTypeRequest parameter )
		{
			using ( var child = container.CreateChildContainer().Extend<DefaultRegistrationsExtension>() )
			{
				var command = child.Resolve<RegisterEntireHierarchyCommand>();
				parameter.Arguments.NotNull().Select( o => new InstanceRegistrationParameter( o ) ).Each( command.ExecuteWith );

				var result = child.TryResolve( parameter.RequestedType );
				return result;
			}
		}
	}
}