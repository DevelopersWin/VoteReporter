using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using PostSharp;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects
{
	[PSerializable]
	[ProvideAspectRole( "Data" ), LinesOfCodeAvoided( 1 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Tracing )]
	public sealed class CreatorAttribute : OnMethodBoundaryAspect
	{
		public override void OnSuccess( MethodExecutionArgs args )
		{
			if ( args.ReturnValue != null )
			{
				args.Instance.As<ICreator>( creator => args.ReturnValue.Set( Creator.Property, creator ) );
			}
		}
	}

	public interface IController
	{
		bool IsAllowed( Func<object, bool> assign, object parameter );

		object Execute( Func<object, object> assign, object parameter );
	}

	class Controller : IController
	{
		readonly IParameterAware workflow;
		readonly IAssignableParameterAware assignable;

		public Controller( IParameterAware workflow, IAssignableParameterAware assignable )
		{
			this.workflow = workflow;
			this.assignable = assignable;
		}

		public bool IsAllowed( Func<object, bool> assign, object parameter )
		{
			using ( new IsAllowedAssignment( assignable, assign ).Configured( false ) )
			{
				return workflow.IsAllowed( parameter );
			}
		}

		public object Execute( Func<object, object> assign, object parameter )
		{
			using ( new ExecuteAssignment( assignable, assign ).Configured( false ) )
			{
				return workflow.Execute( parameter );
			}
		}

		class IsAllowedAssignment : Assignment<Func<object, bool>>
		{
			public IsAllowedAssignment( IAssignableParameterAware assignable, Func<object, bool> first ) : base( assignable.Assign, new Value<Func<object, bool>>( first ) ) {}
		}

		class ExecuteAssignment : Assignment<Func<object, object>>
		{
			public ExecuteAssignment( IAssignableParameterAware assignable, Func<object, object> first ) : base( assignable.Assign, new Value<Func<object, object>>( first ) ) {}
		}
	}

	public class Profile
	{
		public Profile( Type type, string isAllowed, string execute )
		{
			Type = type;
			IsAllowed = isAllowed;
			Execute = execute;
		}

		public Type Type { get; }
		public string IsAllowed { get; }
		public string Execute { get; }
	}

	public interface IControllerFactory
	{
		IController Create( object instance );
	}

	abstract class ControllerFactoryBase<T> : IControllerFactory
	{
		public IController Create( object instance )
		{
			var aware = instance.AsTo<T, IParameterAware>( Create );
			var state = instance.Get( WorkflowState.Property );
			var assignable = new AssignableParameterAware( aware );
			var result = new Controller( new ParameterWorkflow( state, assignable ), assignable );
			return result;
		}

		protected abstract IParameterAware Create( T instance );
	}

	class WorkflowState : AttachedProperty<object, IParameterWorkflowState>
	{
		public static WorkflowState Property { get; } = new WorkflowState();

		WorkflowState() : base( key => new ParameterWorkflowState() ) {}
	}

	abstract class GenericControllerFactoryBase : IControllerFactory
	{
		readonly Type genericType;
		readonly string methodName;
		readonly TypeAdapter adapter;

		protected GenericControllerFactoryBase( Type genericType, string methodName = nameof(Create) )
		{
			this.genericType = genericType;
			this.methodName = methodName;
			adapter = GetType().Adapt();
		}

		public IController Create( object instance )
		{
			var arguments = instance.GetType().Adapt().GetTypeArgumentsFor( genericType );
			var result = adapter.Invoke<IController>( methodName, arguments, instance );
			return result;
		}
	}

	class GenericFactoryControllerFactory : GenericControllerFactoryBase
	{
		public static GenericFactoryControllerFactory Instance { get; } = new GenericFactoryControllerFactory();

		GenericFactoryControllerFactory() : base( typeof(IFactory<,>), nameof(Create) ) {}

		static IController Create<TParameter, TResult>( object instance ) => FactoryControllerFactory<TParameter, TResult>.Instance.Create( instance );
	}
	class GenericCommandControllerFactory : GenericControllerFactoryBase
	{
		public static GenericCommandControllerFactory Instance { get; } = new GenericCommandControllerFactory();

		GenericCommandControllerFactory() : base( typeof(ICommand<>), nameof(Create) ) {}

		static IController Create<T>( object instance ) => CommandControllerFactory<T>.Instance.Create( instance );
	}

	class FactoryControllerFactory<TParameter, TResult> : ControllerFactoryBase<IFactory<TParameter, TResult>>
	{
		public static FactoryControllerFactory<TParameter, TResult> Instance { get; } = new FactoryControllerFactory<TParameter, TResult>();

		protected override IParameterAware Create( IFactory<TParameter, TResult> instance ) => new FactoryParameterAware<TParameter, TResult>( instance );
	}

	class CommandControllerFactory<T> : ControllerFactoryBase<ICommand<T>>
	{
		public static CommandControllerFactory<T> Instance { get; } = new CommandControllerFactory<T>();

		protected override IParameterAware Create( ICommand<T> instance ) => new CommandParameterAware<T>( instance );
	}

	class CommandControllerFactory : ControllerFactoryBase<ICommand>
	{
		public static CommandControllerFactory Instance { get; } = new CommandControllerFactory();

		protected override IParameterAware Create( ICommand instance ) => new CommandParameterAware( instance );
	}

	class FactoryControllerFactory : ControllerFactoryBase<IFactoryWithParameter>
	{
		public static FactoryControllerFactory Instance { get; } = new FactoryControllerFactory();

		protected override IParameterAware Create( IFactoryWithParameter instance ) => new FactoryWithParameterAware( instance );
	}

	
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ParameterValidatorBase : TypeLevelAspect //, IAspectProvider
	{
		readonly Profile profile;
		readonly IControllerFactory factory;
		/*readonly IResourceRepository<ResourceKey, IControllerFactory> repository;*/

		protected ParameterValidatorBase( Profile profile, IControllerFactory factory ) /*: this( profile, factory, ResourceRepository.Instance ) {}

		protected ParameterValidatorBase( Profile profile, IControllerFactory factory, IResourceRepository<ResourceKey, IControllerFactory> repository )*/
		{
			this.profile = profile;
			this.factory = factory;
			// this.repository = repository;
			// MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"Constructor! {profile.Type}", null, null, null ));
		}

		/*public override bool CompileTimeValidate( Type type )
		{
			var result = !type.GetTypeInfo().IsAbstract || !profile.Type.GetTypeInfo().ContainsGenericParameters;
			return result;
		}*/

		// public override void RuntimeInitialize( Type type ) => repository.Add( new ResourceKey( type, profile.Type ), factory );

		IEnumerable<Tuple<MethodInfo, MethodInfo>> Maps { get; set; }

		public override void CompileTimeInitialize( Type type, AspectInfo aspectInfo )
		{
			var implementation = type.Adapt().DetermineImplementation( profile.Type );
			var map = type.GetTypeInfo().GetRuntimeInterfaceMap( implementation );
			Maps = map.InterfaceMethods.TupleWith( map.TargetMethods );
		}

		/*[OnMethodInvokeAdvice, MethodPointcut( nameof(FindIsAllowed) )]
		public void IsAllowed( MethodInterceptionArgs args )
		{
			/*var controller = factory.Create( args.Instance );
			args.ReturnValue = controller.IsAllowed( o => args.GetReturnValue<bool>(), args.Arguments.Single() );#1#
			// args.ReturnValue = true;
		}

		IEnumerable<MethodInfo> FindIsAllowed( Type type ) => Locate( type, profile.IsAllowed );*/
			
		/*if ( )
			/*var map = Maps.Single( m => m.Item1.Name == profile.IsAllowed );
			// var findIsAllowed = map.Item2;
			var b = profile.Type.GetTypeInfo().IsGenericType; // && type.GetTypeInfo().ContainsGenericParameters;
			var methodInfo = /*b ? map.Item2.DeclaringType.GetGenericTypeDefinition().GetRuntimeMethods().First( info => info.Name == map.Item2.Name ) :#1# map.Item2;
			// MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"{this} {profile.Type} - {profile.Type.GetTypeInfo().IsGenericType} - {type} -  {type.GetTypeInfo().ContainsGenericParameters} - {methodInfo}", null, null, null ));
			yield return methodInfo;*/

		IEnumerable<MethodInfo> Locate( Type type, string name )
		{
			var map = Maps.Single( m => m.Item1.Name == name );
			var defined = map.Item2.DeclaringType == type && !map.Item2.IsAbstract && ( map.Item2.IsFinal || map.Item2.IsVirtual /*|| ( map.Item2.Attributes & MethodAttributes.NewSlot ) == 0*/ );
			// var result = defined ? map.Item2 : null;
			if ( defined )
			{
				// var methodInfo = /*b ? map.Item2.DeclaringType.GetGenericTypeDefinition().GetRuntimeMethods().First( info => info.Name == map.Item2.Name ) :*/ map.Item2;
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"{this} {name}: {type} ({map.Item2})", null, null, null ) );
				yield return map.Item2;
			}
		}

		[OnMethodInvokeAdvice, MethodPointcut( nameof(FindExecute) )]
		public void OnExecute( MethodInterceptionArgs args )
		{
			// args.Instance.As<ICommand<MethodBase>>( Create );

			// var controller = factory.Create( args.Instance );
			// args.ReturnValue = controller.Execute( o => args.GetReturnValue(), args.Arguments.Single() );
		}

		/*public void Create( ICommand<MethodBase> instance )
		{
			// var aware = new CommandParameterAware<MethodBase>( instance );
			// var state = instance.Get( WorkflowState.Property );
			// var assignable = new AssignableParameterAware( aware );
			//var result = new Controller( new ParameterWorkflow( state, assignable ), assignable );
			// return result;
		}*/

		IEnumerable<MethodInfo> FindExecute( Type type ) => Locate( type, profile.Execute );

		/*IEnumerable<MethodInfo> FindExecute( Type type )
		{
			var map = Maps.Single( m => m.Item1.Name == profile.Execute );
			var defined = map.Item2.DeclaringType == type && ( map.Item2.IsFinal || ( map.Item2.Attributes & MethodAttributes.NewSlot ) == 0 );
			if ( defined )
			{
				// var methodInfo = /*b ? map.Item2.DeclaringType.GetGenericTypeDefinition().GetRuntimeMethods().First( info => info.Name == map.Item2.Name ) :#1# map.Item2;
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"{this} FindExecute: {type} ({map.Item2})", null, null, null ));
				
				yield return map.Item2;
			}
			
			// var findIsAllowed = map.Item2;
			
			// yield break;
		}*/

		/*public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			var result = targetElement.AsTo<Type, IEnumerable<AspectInstance>>( type =>
			{
				var implementation = type.Adapt().DetermineImplementation( profile.Type );
				// MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"{type.GetTypeInfo().IsAbstract}: {this} - {type} - {implementation}", null, null, null ));
				
				var map = type.GetTypeInfo().GetRuntimeInterfaceMap( implementation );
				var methods = map.InterfaceMethods.TupleWith( map.TargetMethods );
				/*var mapped = methods.Single( tuple => tuple.Item1.Name == profile.IsAllowed );
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {this} {profile.Type}: {map.InterfaceType} -> {map.TargetType}", null, null, null ));#1#
				var i = new[] { profile.IsAllowed, profile.Execute }
							.Select( s => methods.Where( m => m.Item1.Name == s ).Select( m => type.IsConstructedGenericType ? m.Item2.DeclaringType.GetGenericTypeDefinition().GetRuntimeMethods().First( info => info.Name == m.Item2.Name ) : m.Item2 ).Single() )
							.TupleWith( new IAspect[] { new IsAllowedAspect( profile.Type ), new ExecuteAspect( profile.Type ) } ).ToArray();

				
				var first = i.First();
				var findIsAllowed = first.Item1;
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"{this} {findIsAllowed.DeclaringType}.{findIsAllowed} -> {first.Item2}", null, null, null ));
				var items = i.Select( info => new AspectInstance( info.Item1, info.Item2 ) );
				return items;
			} );
			return result;
		}*/
	}

	public sealed class FactoryParameterValidator : ParameterValidatorBase
	{
		public FactoryParameterValidator() : 
			base( new Profile( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), FactoryControllerFactory.Instance ) {}
	}

	public sealed class GenericFactoryParameterValidator : ParameterValidatorBase
	{
		public GenericFactoryParameterValidator() : 
			base( new Profile( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), GenericFactoryControllerFactory.Instance ) {}
	}

	public sealed class CommandParameterValidator : ParameterValidatorBase
	{
		public CommandParameterValidator() : 
			base( new Profile( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ), CommandControllerFactory.Instance ) {}
	}

	// [Serializable]
	public sealed class GenericCommandParameterValidator : ParameterValidatorBase
	{
		public GenericCommandParameterValidator() : 
			base( new Profile( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ), GenericCommandControllerFactory.Instance ) {}
	}

	/*[PSerializable]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	public abstract class ParameterValidationMethodBase : MethodInterceptionAspect
	{
		protected ParameterValidationMethodBase( Type implementationType )
		{
			ImplementationType = implementationType;
		}

		Type ImplementationType { get; set; }

		IControllerFactory Factory { get; set; }

		public override void RuntimeInitialize( MethodBase method )
		{
			var key = new ResourceKey( method.DeclaringType, ImplementationType );
			ResourceRepository.Instance
				//.OfType<Type>()
				.Where( o => key == o )
				.Take( 1 )
				.Subscribe( resourceKey =>
				{
					Factory = ResourceRepository.Instance.Get( resourceKey );
				} );
		}

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = Factory.Create( args.Instance );
			args.ReturnValue = GetValue( controller, args.GetReturnValue, args.Arguments.Single() );
		}

		protected abstract object GetValue( Controller controller, Func<object> factory, object parameter );
	}

	public sealed class IsAllowedAspect : ParameterValidationMethodBase
	{
		public IsAllowedAspect( Type implementationType ) : base( implementationType ) {}

		// public IsAllowedAspect( IControllerFactory factory ) : base( factory ) {}
		protected override object GetValue( Controller controller, Func<object> factory, object parameter ) => 
			controller.IsAllowed( o => (bool)factory(), parameter );
	}

	public sealed class ExecuteAspect : ParameterValidationMethodBase
	{
		public ExecuteAspect( Type implementationType ) : base( implementationType ) {}

		protected override object GetValue( Controller controller, Func<object> factory, object parameter ) => controller.Execute( o => factory(), parameter );
	}*/

	class AssignableParameterAware : IAssignableParameterAware
	{
		readonly IParameterAware inner;

		public AssignableParameterAware( IParameterAware inner )
		{
			this.inner = inner;
		}

		public void Assign( Func<object, bool> condition ) => Condition = condition;

		public void Assign( Func<object, object> execute ) => Factory = execute;

		Func<object, bool> Condition { get; set; }

		Func<object, object> Factory { get; set; }

		public bool IsAllowed( object parameter )
		{
			var condition = Condition ?? inner.IsAllowed;
			return condition( parameter );
		}

		public object Execute( object parameter )
		{
			var factory = Factory ?? inner.Execute;
			return factory( parameter );
		}
	}

	public interface IAssignableParameterAware : IParameterAware
	{
		void Assign( Func<object, bool> condition );

		void Assign( Func<object, object> execute );
	}


	/*public class ParameterValidationAttribute : InstanceLevelAspect
	{
		/*readonly Type interfaceType;
		readonly string methodBaseName;

		public ParameterValidationAttribute( Type interfaceType, string methodBaseName )
		{
			this.interfaceType = interfaceType;
			this.methodBaseName = methodBaseName;
		}#1#

		public override void RuntimeInitializeInstance()
		{
			base.RuntimeInitializeInstance();

		}
	}

	

	/*class Validator
	{
		readonly object instance;
		readonly MethodBase method;
		readonly object[] arguments;
		readonly ConditionMonitor monitor;

		public Validator( object instance, MethodBase method, Arguments arguments ) : this( instance, method, arguments.ToArray(), new Check( method, KeyFactory.Instance.CreateUsing( method, instance, arguments ).ToString() ).Value ) {}

		Validator( object instance, MethodBase method, object[] arguments, ConditionMonitor monitor )
		{
			this.instance = instance;
			this.method = method;
			this.arguments = arguments;
			this.monitor = monitor;
		}

		public void Mark( bool result )
		{
			monitor.Reset();
			if ( result )
			{
				monitor.Apply();
			}
		}

		public bool IsValid()
		{
			var result = monitor.IsApplied || (bool)method.Invoke( instance, arguments );
			monitor.Reset();
			return result;
		}

		class Check : Checked
		{
			public Check( MethodBase source, string key ) : base( source, key ) {}
		}
	}#1#

	[AttributeUsage( AttributeTargets.Class )]
	public abstract class AspectAttributeBase : Attribute
	{
		protected AspectAttributeBase( bool enabled )
		{
			Enabled = enabled;
		}

		public bool Enabled { get; }
	}

	public class AutoValidationAttribute : AspectAttributeBase
	{
		public AutoValidationAttribute( bool enabled ) : base( enabled ) {}
	}

	public class ParameterValidator : IParameterValidator
	{
		public static ParameterValidator Instance { get; } = new ParameterValidator();
		ParameterValidator() {}

		public bool IsValid( IsValidParameter parameter )
		{
			var deferred = new Lazy<bool>( parameter.Result );
			var monitor = new Checked( parameter.Instance, parameter.Arguments ).Value;
			var result = monitor.IsApplied || deferred.Value;
			monitor.Reset();
			if ( deferred.IsValueCreated && deferred.Value )
			{
				monitor.Apply();
			}
			return result;
		}
	}

	public class IsValidParameter : ValidateParameterBase<bool>
	{
		public IsValidParameter( object instance, object[] arguments, Func<bool> result ) : base( instance, arguments, result ) {}
	}

	public abstract class ValidateParameterBase<T>
	{
		protected ValidateParameterBase( object instance, object[] arguments, Func<T> result )
		{
			Instance = instance;
			Arguments = arguments;
			Result = result;
		}

		public object Instance { get; }
		public object[] Arguments { get; }

		public Func<T> Result { get; }
	}

	public class ValidateParameter : ValidateParameterBase<object>
	{
		public ValidateParameter( object instance, object[] arguments, Func<object> result ) : base( instance, arguments, result ) {}
	}

	public class ParameterValidation : IParameterValidation
	{
		readonly MethodBase reference;
		readonly string name;
		
		public ParameterValidation( MethodBase reference, string name )
		{
			this.reference = reference;
			this.name = name;
		}

		public object GetValidatedValue( ValidateParameter parameter )
		{
			var store = new Validated( parameter.Instance );
			var validated = store.Value;
			// var stop = !validated && /*parameter.Instance is FactoryTypeFactory &&#1# parameter.Arguments.FirstOrDefault().AsTo<Type, bool>( type => type.Name.Contains( "RegistrationSupportTests" ) );
			var result = validated || Validate( parameter ) ? AsValidated( store, parameter.Result ) : null;
			return result;
		}

		bool Validate( ValidateParameter parameter )
		{
			var type = parameter.Instance.GetType();
			var mapped = new MethodLocator( reference ).Create( type );
			var itemName = name ?? $"Can{mapped.Name.ToStringArray( '.' ).Last()}";
			var validator = new MethodLocator( mapped, itemName ).Create( type );
			var result = (bool)validator.Invoke( parameter.Instance, parameter.Arguments );
			return result;
		}

		static object AsValidated( Validated store, Func<object> factory )
		{
			using ( new AssignValidation( store ).AsExecuted( true ) )
			{
				return factory();
			}
		}

		[AutoValidation( false )/*, AssociatedDispose( false )#1#]
		class AssignValidation : AssignValueCommand<bool>
		{
			public AssignValidation( IWritableStore<bool> store ) : base( store ) {}
		}

		// [AssociatedDispose( false )]
		class Validated : ThreadAmbientStore<bool>
		{
			public Validated( object instance ) : base( instance.GetHashCode().ToString() ) {}
		}
	}

	public interface IParameterValidator
	{
		bool IsValid( IsValidParameter parameter );
	}

	public interface IParameterValidation
	{
		object GetValidatedValue( ValidateParameter parameter );
	}

	[AutoValidation( false )]
	class MethodLocator : FactoryBase<Type, MethodInfo>
	{
		readonly Type[] types;
		readonly MethodBase reference;
		readonly string name;

		public MethodLocator( MethodBase reference ) : this( reference, reference.Name ) {}

		public MethodLocator( MethodBase reference, string name ) : this( reference, name, GetParameterTypes( reference ) ) {}

		public MethodLocator( MethodBase reference, string name, Type[] types )
		{
			this.reference = reference;
			this.name = name;
			this.types = types;
		}

		public override MethodInfo Create( Type parameter )
		{
			var parameters = reference.ContainsGenericParameters ? ParameterTypeLocator.Instance.Create( parameter ).With( type => type.ToItem(), () => types ) : types;
			var result = parameter.GetRuntimeMethods().FirstOrDefault( info => info.Name == name && GetParameterTypes( info ).SequenceEqual( parameters ) )
						 ?? FromInterface( parameter, types );
			return result;
		}

		static Type[] GetParameterTypes( MethodBase @this ) => @this.GetParameters().Select( parameterInfo => parameterInfo.ParameterType ).ToArray();

		MethodInfo FromInterface( Type parameter, Type[] parameterTypes ) => 
			parameter.Adapt()
					 .GetAllInterfaces()
					 .Select( parameter.GetTypeInfo().GetRuntimeInterfaceMap )
					 .SelectMany( mapping => mapping.TargetMethods.Select( info => new { info, mapping.InterfaceType } ) )
					 .Where( item => /*item.info.ReturnType == reference &&#1# Contains( item.InterfaceType, item.info ) && GetParameterTypes( item.info ).SequenceEqual( parameterTypes ) )
					 .WithFirst( item => item.info );

		bool Contains( Type interfaceType, MemberInfo candidate ) => new[] { name, $"{interfaceType.FullName}.{name}" }.Contains( candidate.Name );
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class ValidatorAttribute : MethodInterceptionAspect
	{
		readonly IParameterValidator validator;

		public ValidatorAttribute() : this( ParameterValidator.Instance ) {}
		public ValidatorAttribute( IParameterValidator validator )
		{
			this.validator = validator;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var parameter = new IsValidParameter( args.Instance, args.Arguments.ToArray(), args.GetReturnValue<bool> );
			args.ReturnValue = validator.IsValid( parameter );
		}
	}

	public class AspectSupport
	{
		// public static AspectSupport Instance { get; } = new AspectSupport();

		[Freeze]
		public static bool IsEnabled<T>( Type type, bool defaultValue = true ) where T : AspectAttributeBase 
			=> type.GetTypeInfo().GetCustomAttribute<T>( true ).With( attribute => attribute.Enabled, () => defaultValue );
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class AutoValidateAttribute : MethodInterceptionAspect
	{
		readonly string validatingMethodName;

		public AutoValidateAttribute() {}

		public AutoValidateAttribute( [Optional]string validatingMethodName )
		{
			this.validatingMethodName = validatingMethodName;
		}
		IParameterValidation Validator { get; set; }

		public override bool CompileTimeValidate( MethodBase method ) => AspectSupport.IsEnabled<AutoValidationAttribute>( method.DeclaringType );

		public override void RuntimeInitialize( MethodBase method ) => Validator = new ParameterValidation( method, validatingMethodName );

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var enabled = AspectSupport.IsEnabled<AutoValidationAttribute>( args.Instance.GetType() );
			if ( enabled )
			{
				var parameter = new ValidateParameter( args.Instance, args.Arguments.ToArray(), args.GetReturnValue );
				var result = Validator.GetValidatedValue( parameter );
				args.ApplyReturnValue( result );
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}*/
}
