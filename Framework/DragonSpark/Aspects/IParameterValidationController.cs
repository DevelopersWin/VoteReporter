using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using PostSharp;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects
{
	public interface IParameterValidationController
	{
		bool IsValid( object parameter );

		void MarkValid( object parameter, bool valid );

		object Execute( RelayParameter parameter );
	}

	public class ParameterValidationController : IParameterValidationController
	{
		readonly IParameterValidator validator;
		readonly EnabledState validated = new EnabledState();

		public ParameterValidationController( IParameterValidator validator )
		{
			this.validator = validator;
		}

		public bool IsValid( object parameter ) => validated.IsEnabled( parameter );

		public void MarkValid( object parameter, bool valid ) => validated.Enable( parameter, valid );

		protected virtual bool PerformValidation( object parameter ) => validator.IsValid( parameter );

		public virtual object Execute( RelayParameter parameter ) => IsValid( parameter.Parameter ) || PerformValidation( parameter.Parameter ) ? Proceed( parameter ) : null;

		protected object Proceed( RelayParameter parameter )
		{
			var result = parameter.Proceed<object>();
			validated.Enable( parameter.Parameter, false );
			return result;
		}
	}

	public interface IGenericParameterValidator : IParameterValidator
	{
		bool Handles( object parameter );

		object Execute( object parameter );
	}

	public sealed class GenericParameterValidationController : ParameterValidationController, IGenericParameterValidationController
	{
		readonly IGenericParameterValidator generic;

		public GenericParameterValidationController( IGenericParameterValidator generic, IParameterValidator validator ) : base( validator )
		{
			this.generic = generic;
		}

		protected override bool PerformValidation( object parameter ) => generic.Handles( parameter ) ? generic.IsValid( parameter ) : base.PerformValidation( parameter );

		public override object Execute( RelayParameter parameter )
		{
			var result = generic.Handles( parameter.Parameter ) ? generic.Execute( parameter.Parameter ) : base.Execute( parameter );
			return result;
		}

		public object ExecuteGeneric( RelayParameter parameter ) => base.Execute( parameter );
	}

	public sealed class ValidatedGenericCommand : ValidatedParameterAspect
	{
		public ValidatedGenericCommand() : base( new GenericParameterValidationControllerFactory( GenericCommandParameterAdapterStore.Instance, CommandParameterAdapterStore.Instance ) ) {}

		class ProfileProvider : ProviderBase
		{
			public static ProfileProvider Instance { get; } = new ProfileProvider();

			ProfileProvider() : base( new Profile( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ) ) {}
		}

		public override IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => ProfileProvider.Instance.ProvideAspects( targetElement );
	}

	public sealed class ValidatedFactory : ValidatedParameterAspect
	{
		public ValidatedFactory() : base( new ParameterValidationControllerFactory( FactoryParameterAdapterStore.Instance ) ) {}

		class ProfileProvider : ProviderBase
		{
			public static ProfileProvider Instance { get; } = new ProfileProvider();

			ProfileProvider() : base( new Profile( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ) ) {}
		}

		public override IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => ProfileProvider.Instance.ProvideAspects( targetElement );
	}

	public sealed class ValidatedGenericFactory : ValidatedParameterAspect
	{
		public ValidatedGenericFactory() : base( new GenericParameterValidationControllerFactory( GenericFactoryParameterAdapterStore.Instance, FactoryParameterAdapterStore.Instance ) ) {}

		class ProfileProvider : ProviderBase
		{
			public static ProfileProvider Instance { get; } = new ProfileProvider();

			ProfileProvider() : base( new Profile( typeof(IFactory<>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ) ) {}
		}

		public override IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => ProfileProvider.Instance.ProvideAspects( targetElement );
	}

	public sealed class ValidatedCommand : ValidatedParameterAspect
	{
		public ValidatedCommand() : base( new ParameterValidationControllerFactory( CommandParameterAdapterStore.Instance ) ) {}

		class ProfileProvider : ProviderBase
		{
			public static ProfileProvider Instance { get; } = new ProfileProvider();

			ProfileProvider() : base( new Profile( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ) ) {}
		}

		public override IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => ProfileProvider.Instance.ProvideAspects( targetElement );
	}

	public abstract class ProviderBase : IAspectProvider
	{
		readonly Profile profile;

		protected ProviderBase( Profile profile )
		{
			this.profile = profile;
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			var type = targetElement as Type;
			if ( type != null )
			{
				var implementedType = profile.Type;
				var types = implementedType.Append( implementedType.GetTypeInfo().IsGenericTypeDefinition ? implementedType.GetTypeInfo().ImplementedInterfaces : Items<Type>.Default );
				foreach ( var check in types )
				{
					var isGeneric = check.GetTypeInfo().IsGenericTypeDefinition;
					foreach ( var pair in type.Adapt().GetMappedMethods( check ) )
					{
						if ( pair.Item2.DeclaringType == type && !pair.Item2.IsAbstract && ( pair.Item2.IsFinal || pair.Item2.IsVirtual ) )
						{
							var aspect = pair.Item1.Name == profile.IsValid ? ValidatorAspect.Instance :
										 pair.Item1.Name == profile.Execute ? isGeneric ? GenericExecutionAspect.Instance : ExecutionAspect.Instance
										 : default(IAspect);
							/*if ( type.Name == "ExtendedFactory" && aspect != null )
							{
								MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {pair.Item2} : {aspect}", null, null, null ));
							}*/
							if ( aspect != null )
							{
								yield return new AspectInstance( pair.Item2, aspect );
							}
						}
					}
				}
			}
		}
	}

	[PSerializable]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ParameterValidatorAspectBase : MethodInterceptionAspect
	{
		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = ParameterValidation.Controller.Get( args.Instance );
			if ( controller != null )
			{
				var parameter = new RelayParameter( args );
				Execute( controller, parameter );
			}
			else
			{
				throw new InvalidOperationException( $"Controller not set for {args.Instance}" );
			}
		}

		protected abstract void Execute( IParameterValidationController controller, RelayParameter parameter );
	}

	[PSerializable]
	public class ExecutionAspect : ParameterValidatorAspectBase
	{
		public static ExecutionAspect Instance { get; } = new ExecutionAspect();

		protected override void Execute( IParameterValidationController controller, RelayParameter parameter ) => controller.Execute( parameter );
	}

	[PSerializable]
	public sealed class GenericExecutionAspect : ExecutionAspect
	{
		public new static GenericExecutionAspect Instance { get; } = new GenericExecutionAspect();

		protected override void Execute( IParameterValidationController controller, RelayParameter parameter )
		{
			var generic = controller as IGenericParameterValidationController;
			if ( generic != null )
			{
				var temp = generic.ExecuteGeneric( parameter );
				Debugger.Break();
			}
			else
			{
				throw new InvalidOperationException( "Expecting a generic controller." );
			}
		}
	}

	[PSerializable]
	public class ValidatorAspect : ParameterValidatorAspectBase
	{
		public static ValidatorAspect Instance { get; } = new ValidatorAspect();

		protected override void Execute( IParameterValidationController controller, RelayParameter parameter )
		{
			controller.MarkValid( parameter.Parameter, parameter.Proceed<bool>() );

			/*var isValid = controller.IsValid( parameter.Parameter );
			if ( !isValid && parameter.Proceed<bool>() )
			{
				controller.MarkValid( parameter.Parameter, true );
			}
			else
			{
				parameter.Assign( isValid );
			}*/
		}
	}

	public static class ParameterValidation
	{
		public static IAttachedProperty<IParameterValidationController> Controller { get; } = new ThreadLocalAttachedProperty<IParameterValidationController>();
	}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ValidatedParameterAspect : InstanceLevelAspect, IAspectProvider
	{
		readonly IParameterValidationControllerFactory factory;

		protected ValidatedParameterAspect( IParameterValidationControllerFactory factory )
		{
			this.factory = factory;
		}

		public override void RuntimeInitializeInstance() => ParameterValidation.Controller.Set( Instance, factory.Create( Instance ) );

		public abstract IEnumerable<AspectInstance> ProvideAspects( object targetElement );
	}

	public interface IParameterValidationControllerFactory
	{
		IParameterValidationController Create( object instance );
	}

	class ParameterValidationControllerFactory : IParameterValidationControllerFactory
	{
		readonly IAttachedPropertyStore<object, IParameterValidator> store;

		public ParameterValidationControllerFactory( IAttachedPropertyStore<object, IParameterValidator> store )
		{
			this.store = store;
		}

		public IParameterValidationController Create( object instance ) => new ParameterValidationController( store.Create( instance ).Value );
	}

	class GenericParameterValidationControllerFactory : IParameterValidationControllerFactory
	{
		readonly IAttachedPropertyStore<object, IGenericParameterValidator> generic;
		readonly IAttachedPropertyStore<object, IParameterValidator> store;

		public GenericParameterValidationControllerFactory( IAttachedPropertyStore<object, IGenericParameterValidator> generic, IAttachedPropertyStore<object, IParameterValidator> store )
		{
			this.generic = generic;
			this.store = store;
		}

		public IParameterValidationController Create( object instance ) => new GenericParameterValidationController( generic.Create( instance ).Value, store.Create( instance ).Value );
	}

	public interface IGenericParameterValidationController : IParameterValidationController
	{
		object ExecuteGeneric( RelayParameter parameter );
	}

	
}