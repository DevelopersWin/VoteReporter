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
using System.Collections.Immutable;
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
		readonly protected static object Null = new object();

		readonly IParameterValidator validator;
		readonly IWritableStore<IDictionary<object, bool>> validated = new ThreadLocalStore<IDictionary<object, bool>>( () => new Dictionary<object, bool>() );
		
		public ParameterValidationController( IParameterValidator validator )
		{
			this.validator = validator;
		}

		public bool IsValid( object parameter )
		{
			var key = parameter ?? Null;
			var result = validated.Value.ContainsKey( key ) && validated.Value[ key ];
			return result;
		}

		public void MarkValid( object parameter, bool valid ) => validated.Value[parameter ?? Null] = valid;

		protected virtual bool PerformValidation( object parameter ) => validator.IsValid( parameter );

		public virtual object Execute( RelayParameter parameter ) => IsValid( parameter.Parameter ) || PerformValidation( parameter.Parameter ) ? Proceed( parameter ) : null;

		protected object Proceed( RelayParameter parameter )
		{
			var result = parameter.Proceed<object>();
			validated.Value.Clear();
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

		readonly IWritableStore<IDictionary<object, bool>> active = new ThreadLocalStore<IDictionary<object, bool>>( () => new Dictionary<object, bool>() );

		public GenericParameterValidationController( IGenericParameterValidator generic, IParameterValidator validator ) : base( validator )
		{
			this.generic = generic;
		}

		protected override bool PerformValidation( object parameter ) => generic.Handles( parameter ) ? generic.IsValid( parameter ) : base.PerformValidation( parameter );

		public override object Execute( RelayParameter parameter )
		{
			var dictionary = active.Value;
			var key = parameter.Parameter ?? Null;
			var handle = generic.Handles( parameter.Parameter ) && !dictionary.TryGet( key );
			if ( handle )
			{
				using ( dictionary.Assignment( key, true ) )
				{
					var result = generic.Execute( key );
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
				var parameter = new RelayParameter( args, args.Arguments.GetArgument( 0 ) );
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
	public sealed class GenericExecutionAspect : ExecutionAspect
	{
		public new static GenericExecutionAspect Instance { get; } = new GenericExecutionAspect();
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
		}
	}

	public static class ParameterValidation
	{
		public static ICache<IParameterValidationController> Controller { get; } = new Cache<IParameterValidationController>();
	}

	public sealed class ValidatedCommand : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidationController> Factory = new ParameterValidationControllerFactory( CommandParameterAdapterFactory.Instance.ToDelegate() ).ToDelegate();

		public ValidatedCommand() : base( Factory ) {}

		public class Aspects : SupplementalAspect
		{
			public Aspects() : base( new Profile( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ) ) {}
		}
	}

	public sealed class ValidatedGenericCommand : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidationController> Factory = new GenericParameterValidationControllerFactory( GenericCommandParameterAdapterFactory.Instance.ToDelegate(), CommandParameterAdapterFactory.Instance.ToDelegate() ).ToDelegate();

		public ValidatedGenericCommand() : base( Factory ) {}

		public class Aspects : SupplementalAspect
		{
			public Aspects() : base( new Profile( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ) ) {}
		}
	}

	public sealed class ValidatedFactory : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidationController> Factory = new ParameterValidationControllerFactory( FactoryParameterAdapterFactory.Instance.ToDelegate() ).ToDelegate();

		public ValidatedFactory() : base( Factory ) {}

		public class Aspects : SupplementalAspect
		{
			public Aspects() : base( new Profile( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ) ) {}
		}
	}

	public sealed class ValidatedGenericFactory : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidationController> Factory = new GenericParameterValidationControllerFactory( GenericFactoryParameterAdapterFactory.Instance.ToDelegate(), FactoryParameterAdapterFactory.Instance.ToDelegate() ).ToDelegate();
		
		public ValidatedGenericFactory() : base( Factory ) {}

		public class Aspects : SupplementalAspect
		{
			public Aspects() : base( new Profile( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ) ) {}
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
				var types = implementedType.Append( implementedType.GetTypeInfo().IsGenericTypeDefinition ? implementedType.GetTypeInfo().ImplementedInterfaces : Items<Type>.Default ).ToImmutableArray();

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
		readonly Func<object, IParameterValidationController> factory;
		
		protected ValidatedParameterAspectBase( Func<object, IParameterValidationController> factory )
		{
			this.factory = factory;
		}

		public override void RuntimeInitializeInstance() => ParameterValidation.Controller.Set( Instance, factory( Instance ) );
	}

	public interface IParameterValidationControllerFactory : IFactory<object, IParameterValidationController> {}

	class ParameterValidationControllerFactory : FactoryBase<object, IParameterValidationController>, IParameterValidationControllerFactory
	{
		readonly Func<object, IParameterValidator> create;

		public ParameterValidationControllerFactory( Func<object, IParameterValidator> create )
		{
			this.create = create;
		}

		public override IParameterValidationController Create( object instance ) => new ParameterValidationController( create( instance ) );
	}

	class GenericParameterValidationControllerFactory : FactoryBase<object, IParameterValidationController>, IParameterValidationControllerFactory
	{
		readonly Func<object, IGenericParameterValidator> generic;
		readonly Func<object, IParameterValidator> store;

		public GenericParameterValidationControllerFactory( Func<object, IGenericParameterValidator> generic, Func<object, IParameterValidator> store )
		{
			this.generic = generic;
			this.store = store;
		}

		public override IParameterValidationController Create( object instance ) => new GenericParameterValidationController( generic( instance ), store( instance ) );
	}

	public interface IGenericParameterValidationController : IParameterValidationController
	{
		object ExecuteGeneric( RelayParameter parameter );
	}
}