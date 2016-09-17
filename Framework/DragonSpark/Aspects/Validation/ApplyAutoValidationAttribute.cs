using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects.Validation
{
	public interface IActive : IAssignableSource<bool> {}
	class Active : ThreadLocalStore<bool>, IActive {}

	[IntroduceInterface( typeof(IActive) )]
	[IntroduceInterface( typeof(IAutoValidationController) )]
	[LinesOfCodeAvoided( 4 ), ProvideAspectRole( KnownRoles.EnhancedValidation ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.ParameterValidation )
		]
	public class ApplyAutoValidationAttribute : ApplyAspectBase, IAutoValidationController, IActive
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = new AspectInstances( Defaults.Profiles.AsEnumerable() ).ToSourceDelegate();
		readonly static Func<Type, bool> Specification = new Specification( Defaults.Adapters ).ToSpecificationDelegate();

		readonly IActive active = new Active();

		public ApplyAutoValidationAttribute() : this( Specification, DefaultSource ) {}
		protected ApplyAutoValidationAttribute( Func<Type, bool> specification, Func<Type, IEnumerable<AspectInstance>> source ) : base( specification, source ) {}

		IAutoValidationController Controller { get; set; }
		public sealed override void RuntimeInitializeInstance() => Controller = Defaults.ControllerSource( Instance );

		bool IAutoValidationController.Handles( object parameter ) => Controller.Handles( parameter );
		void IAutoValidationController.MarkValid( object parameter, bool valid ) => Controller.MarkValid( parameter, valid );
		object IAutoValidationController.Execute( object parameter, IInvocation proceed ) => Controller.Execute( parameter, proceed );

		public bool Get() => active.Get();
		object ISource.Get() => Get();
		void IAssignable<bool>.Assign( bool item ) => active.Assign( item );
	}
}