using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Validation
{
	[IntroduceInterface( typeof(IAutoValidationController) )]
	[LinesOfCodeAvoided( 4 ), ProvideAspectRole( KnownRoles.EnhancedValidation ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.ParameterValidation )
		]
	public sealed class ApplyAutoValidationAttribute : InstanceBasedAspectBase, IAutoValidationController
	{
		readonly IAutoValidationController controller;

		public ApplyAutoValidationAttribute() : base( o => new ApplyAutoValidationAttribute( Defaults.ControllerSource( o ) ), Support.Default ) {}

		[UsedImplicitly]
		public ApplyAutoValidationAttribute( IAutoValidationController controller )
		{
			this.controller = controller;
		}

		bool IAutoValidationController.IsActive => controller.IsActive;
		bool IAutoValidationController.Handles( object parameter ) => controller.Handles( parameter );
		void IAutoValidationController.MarkValid( object parameter, bool valid ) => controller.MarkValid( parameter, valid );
		object IAutoValidationController.Execute( object parameter, IInvocation proceed ) => controller.Execute( parameter, proceed );
	}
}