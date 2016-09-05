using DragonSpark.Application;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.TypeSystem.Generics;
using PostSharp.Extensibility;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Defaults = DragonSpark.Aspects.Validation.Defaults;

namespace DragonSpark.Aspects.Invocation
{
	sealed class AutoValidationValidator<T> : DecoratorFactoryBase<T, bool>
	{
		readonly IAutoValidationController controller;

		public AutoValidationValidator( IAutoValidationController controller )
		{
			this.controller = controller;
		}

		public override IDecorator<T, bool> Get( IDecorator<T, bool> parameter ) => new Context( controller, parameter );

		sealed class Context : IDecorator<T, bool>
		{
			readonly IAutoValidationController controller;
			readonly IDecorator<T, bool> next;

			public Context( IAutoValidationController controller, IDecorator<T, bool> next )
			{
				this.controller = controller;
				this.next = next;
			}

			public bool Execute( T parameter ) => controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, next.Execute( parameter ) );
		}
	}

	sealed class AutoValidationExecutor<TParameter, TResult> : DecoratorFactoryBase<TParameter, TResult>
	{
		readonly IAutoValidationController controller;

		public AutoValidationExecutor( IAutoValidationController controller )
		{
			this.controller = controller;
		}

		public override IDecorator<TParameter, TResult> Get( IDecorator<TParameter, TResult> parameter ) => new Context( controller, parameter );

		sealed class Context : IDecorator<TParameter, TResult>
		{
			readonly IAutoValidationController controller;
			readonly IDecorator<TParameter, TResult> next;

			public Context( IAutoValidationController controller, IDecorator<TParameter, TResult> next )
			{
				this.controller = controller;
				this.next = next;
			}

			public TResult Execute( TParameter parameter ) => controller.Execute( parameter, () => next.Execute( parameter ) ).As<TResult>();
		}
	}

	[MulticastAttributeUsage( PersistMetaData =  true )]
	public class ApplyAutoValidationAttribute : ApplyPolicyAttribute
	{
		public ApplyAutoValidationAttribute() : base( typeof(AutoValidationPolicy) ) {}
	}

	public sealed class AutoValidationPolicy : PolicyBase<object>
	{
		readonly static IGenericMethodContext<Execute> Binder = typeof(AutoValidationPolicy).Adapt().GenericCommandMethods[ nameof(Bind) ];

		public static AutoValidationPolicy Default { get; } = new AutoValidationPolicy();
		AutoValidationPolicy() : this( AutoValidation.DefaultProfiles, Defaults.ControllerSource ) {}

		readonly ImmutableArray<IAspectProfile> profiles;
		readonly Func<object, IAutoValidationController> controllerSource;

		AutoValidationPolicy( ImmutableArray<IAspectProfile> profiles, Func<object, IAutoValidationController> controllerSource )
		{
			this.profiles = profiles;
			this.controllerSource = controllerSource;
		}

		public override void Apply( object parameter )
		{
			var controller = controllerSource( parameter );

			foreach ( var profile in profiles.Introduce( parameter.GetType(), tuple => tuple.Item1.Method.DeclaringType.Adapt().IsAssignableFrom( tuple.Item2 ) ).ToArray() )
			{
				var execute = parameter.GetDelegate( profile.Method );
				Binder.Make( Types.Default.Get( execute.GetMethodInfo() ).ToArray() ).Invoke( parameter, profile, controller, execute );
			}
		}

		static void Bind<TParameter, TResult>( object parameter, IAspectProfile profile, IAutoValidationController controller, Delegate execute ) // TODO: Bleh.
		{
			var specification = parameter.GetDelegate( profile.Validation );
			Repositories<TParameter, bool>.Default.Get( specification ).Add( new AutoValidationValidator<TParameter>( controller ) );
			Repositories<TParameter, TResult>.Default.Get( execute ).Add( new AutoValidationExecutor<TParameter, TResult>( controller ) );
		}
	}
}
