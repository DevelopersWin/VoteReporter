using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	[IntroduceInterface( typeof(IAutoValidationController) )]
	[LinesOfCodeAvoided( 4 ), ProvideAspectRole( KnownRoles.EnhancedValidation ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.ParameterValidation )
		]
	public sealed class ApplyAutoValidationAttribute : ApplyAspectBase, IAutoValidationController
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = new AspectInstances( Defaults.Profiles.AsEnumerable() ).ToSourceDelegate();
		readonly static Func<Type, bool> Specification = new Specification( Defaults.Adapters ).ToSpecificationDelegate();

		public ApplyAutoValidationAttribute() : this( Specification, DefaultSource ) {}

		ApplyAutoValidationAttribute( Func<Type, bool> specification, Func<Type, IEnumerable<AspectInstance>> source ) : base( specification, source ) {}

		public override void RuntimeInitializeInstance() => Controller = Defaults.ControllerSource( Instance );
		IAutoValidationController Controller { get; set; }

		bool IAutoValidationController.IsActive => Controller.IsActive;
		bool IAutoValidationController.Handles( object parameter ) => Controller.Handles( parameter );
		void IAutoValidationController.MarkValid( object parameter, bool valid ) => Controller.MarkValid( parameter, valid );
		object IAutoValidationController.Execute( object parameter, IInvocation proceed ) => Controller.Execute( parameter, proceed );
	}
}