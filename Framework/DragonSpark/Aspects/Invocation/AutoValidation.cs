using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Invocation
{
	sealed class AutoValidationValidator : InvocationFactoryBase<object, bool>
	{
		public static AutoValidationValidator Default { get; } = new AutoValidationValidator();
		AutoValidationValidator() {}

		/*readonly IAutoValidationController controller;

		public AutoValidationValidator( IAutoValidationController controller )
		{
			this.controller = controller;
		}*/

		protected override IInvocation<object, bool> Create( IInvocation<object, bool> parameter ) => new Context( parameter );

		sealed class Context : InvocationBase<object, bool>
		{
			readonly Func<object, IAutoValidationController> controllerSource;
			readonly IInvocation<object, bool> next;

			public Context( IInvocation<object, bool> next ) : this( Validation.Defaults.ControllerSource, next ) {}

			Context( Func<object, IAutoValidationController> controllerSource, IInvocation<object, bool> next )
			{
				this.controllerSource = controllerSource;
				this.next = next;
			}

			public override bool Invoke( object instance, object parameter )
			{
				var controller = controllerSource( instance );
				return controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, next.Invoke( instance, parameter ) );
			}
		}
	}

	sealed class AutoValidationExecutor : InvocationFactoryBase
	{
		public static AutoValidationExecutor Default { get; } = new AutoValidationExecutor();
		AutoValidationExecutor() {}

		/*readonly IAutoValidationController controller;

		public AutoValidationExecutor( IAutoValidationController controller )
		{
			this.controller = controller;
		}*/

		public override IInvocation Get( IInvocation parameter ) => new Context( parameter );

		sealed class Context : IInvocation
		{
			readonly Func<object, IAutoValidationController> controllerSource;
			readonly IInvocation next;

			public Context( IInvocation next ) : this( Validation.Defaults.ControllerSource, next ) {}

			Context( Func<object, IAutoValidationController> controllerSource, IInvocation next )
			{
				this.controllerSource = controllerSource;
				this.next = next;
			}

			public object Invoke( object instance, object parameter ) => controllerSource( instance ).Execute( parameter, () => next.Invoke( instance, parameter ) );
		}
	}

	[MulticastAttributeUsage( PersistMetaData =  true )]
	public class ApplyAutoValidationAttribute : ApplyPoliciesAttribute
	{
		public ApplyAutoValidationAttribute() : base( typeof(AutoValidationPolicy) ) {}

		public override void RuntimeInitializeInstance()
		{
			Validation.Defaults.ControllerSource( Instance );
			base.RuntimeInitializeInstance();
		}
	}

	public sealed class AutoValidationPolicy : PolicyBase
	{
		public static AutoValidationPolicy Default { get; } = new AutoValidationPolicy();
		AutoValidationPolicy() : this( AutoValidation.DefaultProfiles ) {}

		readonly ImmutableArray<IAspectProfile> profiles;

		AutoValidationPolicy( ImmutableArray<IAspectProfile> profiles )
		{
			this.profiles = profiles;
		}

		public override IEnumerable<PolicyMapping> Get( Type parameter )
		{
			foreach ( var profile in profiles.Introduce( parameter, tuple => tuple.Item1.Method.DeclaringType.Adapt().IsAssignableFrom( tuple.Item2 ) ) )
			{
				yield return new PolicyMapping( profile.Validation.Find( parameter ), AutoValidationValidator.Default );
				yield return new PolicyMapping( profile.Method.Find( parameter ), AutoValidationExecutor.Default );
			}
		}
	}
}
