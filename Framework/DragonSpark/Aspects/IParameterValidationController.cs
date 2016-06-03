using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;

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

		readonly EnabledState active = new EnabledState();

		public GenericParameterValidationController( IGenericParameterValidator generic, IParameterValidator validator ) : base( validator )
		{
			this.generic = generic;
		}

		protected override bool PerformValidation( object parameter ) => generic.Handles( parameter ) ? generic.IsValid( parameter ) : base.PerformValidation( parameter );

		public override object Execute( RelayParameter parameter )
		{
			var handle = generic.Handles( parameter.Parameter ) && !active.IsEnabled( parameter.Parameter );
			if ( handle )
			{
				using ( active.Assignment( parameter.Parameter ) )
				{
					var result = generic.Execute( parameter.Parameter );
					return result;
				}
			}
			return base.Execute( parameter );
		}

		public object ExecuteGeneric( RelayParameter parameter ) => base.Execute( parameter );
		
	}

	[PSerializable]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ParameterValidatorAspectBase : MethodInterceptionAspect
	{
		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var property = ParameterValidation.Controller;
			var controller = property.Get( args.Instance );
			if ( controller != null )
			{
				var parameter = new RelayParameter( args );
				args.ReturnValue = Execute( controller, parameter ) ?? args.ReturnValue;
			}
			else
			{
				throw new InvalidOperationException( $"Controller not set for {args.Instance} - {args.Instance.GetHashCode()}" );
			}
		}

		protected abstract object Execute( IParameterValidationController controller, RelayParameter parameter );
	}

	[PSerializable]
	public class ExecutionAspect : ParameterValidatorAspectBase
	{
		public static ExecutionAspect Instance { get; } = new ExecutionAspect();

		protected override object Execute( IParameterValidationController controller, RelayParameter parameter ) => controller.Execute( parameter );
	}

	[PSerializable]
	// [MulticastAttributeUsage( Inheritance = MulticastInheritance.Multicast, TargetMemberAttributes = MulticastAttributes.NonAbstract )]
	public sealed class GenericExecutionAspect : ExecutionAspect
	{
		public new static GenericExecutionAspect Instance { get; } = new GenericExecutionAspect()/* { AttributeInheritance = MulticastInheritance.Multicast, AttributeTargetMemberAttributes = MulticastAttributes.NonAbstract }*/;

		GenericExecutionAspect() {}

		protected override object Execute( IParameterValidationController controller, RelayParameter parameter )
		{
			var generic = controller as IGenericParameterValidationController;
			if ( generic != null )
			{
				return generic.ExecuteGeneric( parameter );
			}
			throw new InvalidOperationException( "Expecting a generic controller." );
		}
	}

	[PSerializable]
	public class ValidatorAspect : ParameterValidatorAspectBase
	{
		public static ValidatorAspect Instance { get; } = new ValidatorAspect();

		protected override object Execute( IParameterValidationController controller, RelayParameter parameter )
		{
			var result = parameter.Proceed<bool>();
			controller.MarkValid( parameter.Parameter, result );
			return null;

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
		public static IAttachedProperty<IParameterValidationControllerFactory> Factory { get; } = new AttachedProperty<IParameterValidationControllerFactory>();

		public static IAttachedProperty<IParameterValidationController> Controller { get; } = new ControllerProperty();
	}

	class ControllerProperty : ThreadLocalAttachedProperty<IParameterValidationController>
	{
		public ControllerProperty() : base( Store.Instance ) {}

		class Store : AttachedPropertyStoreBase<object, IParameterValidationController>
		{
			public static Store Instance { get; } = new Store();

			public override IWritableStore<IParameterValidationController> Create( object instance )
			{
				var factory = ParameterValidation.Factory.Get( instance );
				var result = new ThreadLocalStore<IParameterValidationController>( new CreateContext( factory, instance ).Create );
				return result;
			}

			struct CreateContext
			{
				readonly IParameterValidationControllerFactory factory;
				readonly object instance;

				public CreateContext( IParameterValidationControllerFactory factory, object instance )
				{
					this.factory = factory;
					this.instance = instance;
				}

				public IParameterValidationController Create() => factory.Create( instance );
			}
		}
	}

	public sealed class ValidatedCommand : ValidatedParameterAspectBase
	{
		readonly static IParameterValidationControllerFactory Factory = new ParameterValidationControllerFactory( CommandParameterAdapterStore.Instance );

		public ValidatedCommand() : base( Factory ) {}

		public class Supplemental : SupplementalAspect
		{
			public Supplemental() : base( new Profile( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ) ) {}
		}
	}

	public sealed class ValidatedGenericCommand : ValidatedParameterAspectBase
	{
		readonly static IParameterValidationControllerFactory Factory = new GenericParameterValidationControllerFactory( GenericCommandParameterAdapterStore.Instance, CommandParameterAdapterStore.Instance );

		public ValidatedGenericCommand() : base( Factory ) {}

		public class Supplemental : SupplementalAspect
		{
			public Supplemental() : base( new Profile( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ) ) {}
		}
	}

	public sealed class ValidatedFactory : ValidatedParameterAspectBase
	{
		readonly static IParameterValidationControllerFactory Factory = new ParameterValidationControllerFactory( FactoryParameterAdapterStore.Instance );

		public ValidatedFactory() : base( Factory ) {}

		public class Supplemental : SupplementalAspect
		{
			public Supplemental() : base( new Profile( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ) ) {}
		}
	}

	public sealed class ValidatedGenericFactory : ValidatedParameterAspectBase
	{
		readonly static IParameterValidationControllerFactory Factory = new GenericParameterValidationControllerFactory( GenericFactoryParameterAdapterStore.Instance, FactoryParameterAdapterStore.Instance );
		
		public ValidatedGenericFactory() : base( Factory ) {}

		public class Supplemental : SupplementalAspect
		{
			public Supplemental() : base( new Profile( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ) ) {}
		}
	}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict, TargetMemberAttributes = MulticastAttributes.NonAbstract | MulticastAttributes.Instance )]
	public class SupplementalAspect : TypeLevelAspect, IAspectProvider
	{
		readonly Profile profile;

		public SupplementalAspect( Profile profile )
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
					
					var mappedMethods = type.Adapt().GetMappedMethods( check );

					foreach ( var pair in mappedMethods )
					{
						if ( pair.Item2.DeclaringType == type && !pair.Item2.IsAbstract && ( pair.Item2.IsFinal || pair.Item2.IsVirtual ) )
						{
							var aspect = pair.Item1.Name == profile.IsValid ? ValidatorAspect.Instance :
										 pair.Item1.Name == profile.Execute ? isGeneric ? GenericExecutionAspect.Instance : ExecutionAspect.Instance
										 : default(IAspect);

							if ( aspect != null )
							{
								// MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"IAspectProvider.ProvideAspects: {pair.Item1} -> {type}.{pair.Item2} : ({aspect})", null, null, null ));

								yield return new AspectInstance( pair.Item2, aspect );
							}
						}
					}
				}
			}
		}
	}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ValidatedParameterAspectBase : InstanceLevelAspect
	{
		readonly IParameterValidationControllerFactory factory;
		
		protected ValidatedParameterAspectBase( IParameterValidationControllerFactory factory )
		{
			this.factory = factory;
		}

		public override void RuntimeInitializeInstance() => ParameterValidation.Factory.Set( Instance, factory );
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