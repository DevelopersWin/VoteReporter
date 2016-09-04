using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using System;

namespace DragonSpark.Aspects.Invocation
{
	sealed class AutoValidationValidator : DecoratorFactoryBase<object, bool>
	{
		readonly IAutoValidationController controller;

		public AutoValidationValidator( IAutoValidationController controller )
		{
			this.controller = controller;
		}

		public override IDecorator<object, bool> Get( IDecorator<object, bool> parameter ) => new Context( controller, parameter );

		sealed class Context : IDecorator<object, bool>
		{
			readonly IAutoValidationController controller;
			readonly IDecorator<object, bool> next;

			public Context( IAutoValidationController controller, IDecorator<object, bool> next )
			{
				this.controller = controller;
				this.next = next;
			}

			public bool Execute( object parameter ) => controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, next.Execute( parameter ) );
		}
	}

	sealed class AutoValidationExecutor : DecoratorFactoryBase<object>
	{
		readonly IAutoValidationController controller;

		public AutoValidationExecutor( IAutoValidationController controller )
		{
			this.controller = controller;
		}

		public override IDecorator<object, object> Get( IDecorator<object, object> parameter ) => new Context( controller, parameter );

		sealed class Context : IDecorator<object>
		{
			readonly IAutoValidationController controller;
			readonly IDecorator<object, object> next;

			public Context( IAutoValidationController controller, IDecorator<object, object> next )
			{
				this.controller = controller;
				this.next = next;
			}

			public object Execute( object parameter ) => controller.Execute( parameter, () => next.Execute( parameter ) );
		}
	}

	public class ApplyAutoValidationAttribute : ApplyPolicyAttribute
	{
		public ApplyAutoValidationAttribute() : base( typeof(AutoValidationPolicy) ) {}
	}

	public sealed class AutoValidationPolicy : PolicyBase<object>
	{
		public static AutoValidationPolicy Default { get; } = new AutoValidationPolicy();
		AutoValidationPolicy() : this( Defaults.ControllerSource, Repositories<object, bool>.Default.Get, Repositories<object>.Default.Get ) {}

		readonly Func<object, IAutoValidationController> controllerSource;
		readonly Func<Delegate, IDecoratorRepository<object, bool>> specificationSource;
		readonly Func<Delegate, IDecoratorRepository<object, object>> executorSource;

		AutoValidationPolicy( Func<object, IAutoValidationController> controllerSource, Func<Delegate, IDecoratorRepository<object, bool>> specificationSource, Func<Delegate, IDecoratorRepository<object, object>> executorSource )
		{
			this.controllerSource = controllerSource;
			this.specificationSource = specificationSource;
			this.executorSource = executorSource;
		}

		public override void Apply( object parameter )
		{
			var controller = controllerSource( parameter );

			foreach ( var profile in Defaults.AspectProfiles.Introduce( parameter.GetType(), tuple => tuple.Item1.Method.DeclaringType.Adapt().IsAssignableFrom( tuple.Item2 ) ) )
			{
				var specification = parameter.GetDelegate( profile.Validation );
				specificationSource( specification ).Add( new AutoValidationValidator( controller ) );

				var execute = parameter.GetDelegate( profile.Method );
				executorSource( execute ).Add( new AutoValidationExecutor( controller ) );
			}
		}
	}
}
