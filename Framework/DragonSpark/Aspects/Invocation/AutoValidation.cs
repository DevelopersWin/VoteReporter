using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
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

		protected override IInvocation<object, bool> Create( IInvocation<object, bool> parameter ) => new Context( parameter );

		sealed class Context : InvocationBase<object, bool>
		{
			readonly IInvocation<object, bool> next;

			public Context( IInvocation<object, bool> next )
			{
				this.next = next;
			}

			public override bool Invoke( IInstancePolicy instance, object parameter )
			{
				var controller = (IAutoValidationController)instance.Hub;
				var result = controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, next.Invoke( instance, parameter ) );
				return result;
			}
		}
	}

	sealed class AutoValidationExecutor : InvocationFactoryBase
	{
		public static AutoValidationExecutor Default { get; } = new AutoValidationExecutor();
		AutoValidationExecutor() {}

		public override IInvocation Get( IInvocation parameter ) => new Context( parameter );

		sealed class Context : IInvocation
		{
			readonly IInvocation next;

			public Context( IInvocation next )
			{
				this.next = next;
			}

			public object Invoke( IInstancePolicy instance, object parameter ) => ((IAutoValidationController)instance.Hub).Execute( parameter, () => next.Invoke( instance, parameter ) );
		}
	}

	[MulticastAttributeUsage( PersistMetaData =  true )]
	public class ApplyAutoValidationAttribute : ApplyPoliciesAttribute
	{
		public ApplyAutoValidationAttribute() : base( typeof(AutoValidationPolicy) ) {}

		public override void RuntimeInitializeInstance()
		{
			var instance = Instance;
			InstancePolicies.Default.Set( instance, new InstancePolicy( (IAspectHub)Validation.Defaults.ControllerSource( instance ), instance ) );
			
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
