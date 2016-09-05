using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Defaults = DragonSpark.Aspects.Validation.Defaults;

namespace DragonSpark.Aspects.Invocation
{
	sealed class AutoValidationValidator : InvocationFactoryBase<object, bool>
	{
		readonly IAutoValidationController controller;

		public AutoValidationValidator( IAutoValidationController controller )
		{
			this.controller = controller;
		}

		protected override IInvocation<object, bool> Create( IInvocation<object, bool> parameter ) => new Context( controller, parameter );

		sealed class Context : InvocationBase<object, bool>
		{
			readonly IAutoValidationController controller;
			readonly IInvocation<object, bool> next;

			public Context( IAutoValidationController controller, IInvocation<object, bool> next )
			{
				this.controller = controller;
				this.next = next;
			}

			public override bool Invoke( object parameter ) => controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, next.Invoke( parameter ) );
		}
	}

	sealed class AutoValidationExecutor : InvocationFactoryBase
	{
		readonly IAutoValidationController controller;

		public AutoValidationExecutor( IAutoValidationController controller )
		{
			this.controller = controller;
		}

		public override IInvocation Get( IInvocation parameter ) => new Context( controller, parameter );

		sealed class Context : IInvocation
		{
			readonly IAutoValidationController controller;
			readonly IInvocation next;

			public Context( IAutoValidationController controller, IInvocation next )
			{
				this.controller = controller;
				this.next = next;
			}

			public object Invoke( object parameter ) => controller.Execute( parameter, () => next.Invoke( parameter ) );
		}
	}

	[MulticastAttributeUsage( PersistMetaData =  true )]
	public class ApplyAutoValidationAttribute : ApplyPolicyAttribute
	{
		public ApplyAutoValidationAttribute() : base( typeof(AutoValidationPolicy) ) {}
	}

	public sealed class AutoValidationPolicy : PolicyBase
	{
		public static AutoValidationPolicy Default { get; } = new AutoValidationPolicy();
		AutoValidationPolicy() : this( AutoValidation.DefaultProfiles, Defaults.ControllerSource ) {}

		readonly ImmutableArray<IAspectProfile> profiles;
		readonly Func<object, IAutoValidationController> controllerSource;

		AutoValidationPolicy( ImmutableArray<IAspectProfile> profiles, Func<object, IAutoValidationController> controllerSource )
		{
			this.profiles = profiles;
			this.controllerSource = controllerSource;
		}

		protected override IEnumerable<InvocationMapping> Get( object parameter )
		{
			var type = parameter.GetType();
			var controller = controllerSource( parameter );
			var validator = new AutoValidationValidator( controller );
			var executor = new AutoValidationExecutor( controller );
			
			foreach ( var profile in profiles.Introduce( parameter.GetType(), tuple => tuple.Item1.Method.DeclaringType.Adapt().IsAssignableFrom( tuple.Item2 ) )/*.ToArray()*/ )
			{
				yield return new InvocationMapping( profile.Validation.Find( type ), validator );
				yield return new InvocationMapping( profile.Method.Find( type ), executor );
			}
		}
	}
}
