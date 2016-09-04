using DragonSpark.Application;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.TypeSystem.Generics;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Immutable;
using System.Windows.Input;
using Activator = DragonSpark.Activation.Activator;

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

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Class )]
	public class ApplyPolicyAttribute : InstanceLevelAspect
	{
		readonly static IGenericMethodContext<Execute> Context = typeof(ApplyPolicyAttribute).Adapt().GenericCommandMethods[nameof(Apply)];

		readonly ImmutableArray<Type> policyTypes;
		public ApplyPolicyAttribute( params Type[] policyTypes )
		{
			this.policyTypes = policyTypes.ToImmutableArray();
		}

		public override void RuntimeInitializeInstance()
		{
			foreach ( var decorator in policyTypes.SelectAssigned( Activator.Default.Get ) )
			{
				Context.Make( Instance.GetType() ).Invoke( decorator, Instance );
			}
		}

		static void Apply<T>( IPolicy<T> decorator, T instance ) => decorator.Apply( instance );
	}

	sealed class AutoValidationPolicy : PolicyBase<ICommand>
	{
		public static AutoValidationPolicy Default { get; } = new AutoValidationPolicy();
		AutoValidationPolicy() : this( AutoValidationControllerFactory.Default.Get, Repositories<object, bool>.Default.Get, Repositories<object>.Default.Get ) {}

		readonly Func<object, IAutoValidationController> controllerSource;
		readonly Func<Delegate, IDecoratorRepository<object, bool>> specificationSource;
		readonly Func<Delegate, IDecoratorRepository<object, object>> executorSource;

		AutoValidationPolicy( Func<object, IAutoValidationController> controllerSource, Func<Delegate, IDecoratorRepository<object, bool>> specificationSource, Func<Delegate, IDecoratorRepository<object, object>> executorSource )
		{
			this.controllerSource = controllerSource;
			this.specificationSource = specificationSource;
			this.executorSource = executorSource;
		}

		public override void Apply( ICommand parameter )
		{
			var controller = controllerSource( parameter );

			var specification = parameter.GetDelegate<ICommand>( nameof(ICommand.CanExecute) );
			specificationSource( specification ).Add( new AutoValidationValidator( controller ) );

			var execute = parameter.GetDelegate<ICommand>( nameof(ICommand.Execute) );
			executorSource( execute ).Add( new AutoValidationExecutor( controller ) );
		}
	}
}
