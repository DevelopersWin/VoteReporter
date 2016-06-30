using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	public static class Services
	{
		public static ICache<IAutoValidationController> Controller { get; } = new Cache<IAutoValidationController>( o => new AutoValidationController( AdapterLocator.Instance.Create( o ) ) );
		// public static ICache<IParameterValidationAdapter> Adapter { get; } = AdapterLocator.Instance.Cached().ToDelegate();

		/*public static ICache<IList<IParameterHandler>> Handlers { get; } = new ListCache<IParameterHandler>();
		public static ICache<InstanceAwareRepository> Instances { get; } = new ActivatedCache<InstanceAwareRepository>();*/
	}

	/*public interface IInstanceScopedAspect : PostSharp.Aspects.IInstanceScopedAspect
	{
		void RuntimeInitializeInstance( object instance );
	}*/
	

	/*[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class InstanceAwareParentAspectBase : InstanceLevelAspect
	{
		public override void RuntimeInitializeInstance()
		{
			var instances = Services.Instances;
			using ( var repository = instances.Get( Instance ) )
			{
				var list = repository.List();
				foreach ( var aspect in list )
				{
					aspect.RuntimeInitializeInstance( Instance );
				}
			}
			instances.Remove( Instance );
		}
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class InstanceAwareChildAspectBase : MethodInterceptionAspect, IInstanceScopedAspect
	{
		public virtual object CreateInstance( AdviceArgs adviceArgs )
		{
			var result = MemberwiseClone();
			Services.Instances.Get( adviceArgs.Instance ).Add( (IInstanceScopedAspect)result );
			return result;
		}

		public virtual void RuntimeInitializeInstance() {}
		public virtual void RuntimeInitializeInstance( object instance ) {}
	}*/

	public interface IAutoValidationWorker : IConnectionWorker
	{
		Type InterfaceType { get; }

		object Invoke( MethodInvocationSingleArgumentParameter parameter );
	}

	public interface IAutoValidationController : IConnectionOwner, IParameterHandlerAware
	{
		bool IsValid( object parameter );

		void MarkValid( object parameter, bool valid );

		object Execute( object parameter );

		// void Register( IRegistrationProvider provider );
	}

	public class AutoValidationController : ConnectionOwnerBase, IAutoValidationController
	{
		readonly CompositeParameterHandler handlers = new CompositeParameterHandler();
		readonly ConcurrentDictionary<int, object> validated = new ConcurrentDictionary<int, object>();
		readonly IParameterValidationAdapter adapter;

		public AutoValidationController( IParameterValidationAdapter adapter )
		{
			this.adapter = adapter;
		}

		public bool IsValid( object parameter ) => CheckValid( parameter ) || AssignValid( parameter );

		bool AssignValid( object parameter )
		{
			var result = adapter.IsValid( parameter );
			MarkValid( parameter, result );
			return result;
		}

		bool CheckValid( object parameter )
		{
			object stored;
			return validated.TryGetValue( Environment.CurrentManagedThreadId, out stored ) && Equals( stored, parameter ?? Placeholders.Null );
		}

		public void MarkValid( object parameter, bool valid )
		{
			if ( valid )
			{
				validated[Environment.CurrentManagedThreadId] = parameter ?? Placeholders.Null;
			}
			else
			{
				object stored;
				validated.TryRemove( Environment.CurrentManagedThreadId, out stored );
			}
		}

		public object Execute( object parameter )
		{
			var result = IsValid( parameter ) ? adapter.Execute( parameter ) : null;
			MarkValid( parameter, false );
			return result;
		}

		public void Register( IRegistrationProvider provider )
		{
			/*foreach ( var worker in Workers )
			{
				if ( provider.IsSatisfiedBy( worker ) )
				{
					var handler = provider.Create( adapter );
					((IParameterHandlerAware)worker).Register( handler );
					break;
				}
			}*/
		}

		public void Register( IParameterHandler handler ) => handlers.Add( handler );
		public bool Handles( object parameter ) => handlers.Handles( parameter );

		public object Handle( object parameter ) => handlers.Handle( parameter );
	}

	public interface IRegistrationProvider : ISpecification<IAutoValidationWorker>
	{
		IParameterHandler Create( IParameterValidationAdapter adapter );
	}

	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class AutoValidationWorkerHostBase : ConnectionWorkerHostBase
	{
		protected AutoValidationWorkerHostBase( IFactory<object, IConnectionWorker> worker ) : base( worker.Cached() ) {}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( Value != null )
			{
				args.ReturnValue = ((IAutoValidationWorker)Value).Invoke( new MethodInvocationSingleArgumentParameter( args.Instance, args.Method, args.Arguments[0], args.GetReturnValue ) ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}

	class WorkerFactory : FactoryBase<object, IConnectionWorker>
	{
		readonly Type interfaceType;
		readonly Func<IAutoValidationController, Type, IConnectionWorker> factory;

		public WorkerFactory( Type interfaceType, Func<IAutoValidationController, Type, IConnectionWorker> factory )
		{
			this.interfaceType = interfaceType;
			this.factory = factory;
		}

		public override IConnectionWorker Create( object parameter )
		{
			var controller = Services.Controller.Get( parameter );
			var result = factory( controller, interfaceType );
			return result;
		}
	}

	class GenericWorkerFactory : FactoryBase<object, IConnectionWorker>
	{
		readonly Type interfaceType;
		readonly Type registrationType;
		readonly Func<IAutoValidationController, Type, Type, IConnectionWorker> factory;

		public GenericWorkerFactory( Type interfaceType, Type registrationType, Func<IAutoValidationController, Type, Type, IConnectionWorker> factory )
		{
			this.interfaceType = interfaceType;
			this.registrationType = registrationType;
			this.factory = factory;
		}

		public override IConnectionWorker Create( object parameter )
		{
			var controller = Services.Controller.Get( parameter );
			var result = factory( controller, interfaceType, registrationType );
			return result;
		}
	}

	public abstract class AutoValidationWorkerBase : ConnectionWorkerBase<IAutoValidationController>, IAutoValidationWorker
	{
		protected AutoValidationWorkerBase( IAutoValidationController owner, Type interfaceType ) : base( owner )
		{
			InterfaceType = interfaceType;
		}

		public Type InterfaceType { get; }

		public abstract object Invoke( MethodInvocationSingleArgumentParameter parameter );
	}

	public class AutoValidationValidationAspect : AutoValidationWorkerHostBase
	{
		public AutoValidationValidationAspect( Type interfaceType ) : base( new WorkerFactory( interfaceType, ( controller, type ) => new AutoValidationValidateWorker( controller, type ) ) ) {}
	}

	public class GenericAutoValidationValidationAspect : AutoValidationWorkerHostBase
	{
		public GenericAutoValidationValidationAspect( Type interfaceType, Type registrationType ) 
			: base( new GenericWorkerFactory( interfaceType, registrationType, ( controller, type, registration ) => new GenericAutoValidationValidateWorker( controller, type, registration ) ) ) {}
	}

	public abstract class AutoValidationValidateWorkerBase : AutoValidationWorkerBase
	{
		protected AutoValidationValidateWorkerBase( IAutoValidationController owner, Type interfaceType ) : base( owner, interfaceType ) {}

		public override object Invoke( MethodInvocationSingleArgumentParameter parameter )
		{
			var result = parameter.Proceed();
			Owner.MarkValid( parameter.Argument, (bool)result );
			return null;
		}
	}

	class AutoValidationValidateWorker : AutoValidationValidateWorkerBase, IParameterHandlerAware
	{
		readonly CompositeParameterHandler handlers = new CompositeParameterHandler();

		public AutoValidationValidateWorker( IAutoValidationController owner, Type interfaceType ) : base( owner, interfaceType ) {}

		public void Register( IParameterHandler handler ) => handlers.Add( handler );
		public bool Handles( object parameter ) => handlers.Handles( parameter );

		public object Handle( object parameter ) => handlers.Handle( parameter );

		/*public override object Invoke( MethodInvocationParameter parameter )
		{
			var handled = handlers.Count > 0 ? handlers.Handle( parameter.Arguments[0] ) : Placeholders.Null;
			var result = handled == Placeholders.Null ? base.Invoke( parameter ) : handled;
			return result;
		}*/
	}

	class GenericAutoValidationValidateWorker : AutoValidationValidateWorkerBase
	{
		readonly IRegistrationProvider provider;

		public GenericAutoValidationValidateWorker( IAutoValidationController owner, Type interfaceType, Type registrationType ) 
			: this( owner, interfaceType, (IRegistrationProvider)SingletonLocator.Instance.Locate( registrationType ) ) {}

		GenericAutoValidationValidateWorker( IAutoValidationController owner, Type interfaceType, IRegistrationProvider provider ) : base( owner, interfaceType )
		{
			this.provider = provider;
		}

		// protected override void OnInitialize() => Owner.Register( provider );

		public override Priority Priority => Priority.High;
	}

	public class AutoValidationExecuteAspect : AutoValidationWorkerHostBase
	{
		public AutoValidationExecuteAspect( Type interfaceType ) : base( new WorkerFactory( interfaceType, ( controller, type ) => new AutoValidationExecuteWorker( controller, type ) ) ) {}
	}

	public class GenericAutoValidationExecuteAspect : AutoValidationWorkerHostBase
	{
		public GenericAutoValidationExecuteAspect( Type interfaceType, Type registrationType ) : base( new GenericWorkerFactory( interfaceType, registrationType, ( controller, type, registration ) => new GenericAutoValidationExecuteWorker( controller, type, registration ) ) ) {}
	}

	public class AutoValidationExecuteWorkerBase : AutoValidationWorkerBase
	{
		public AutoValidationExecuteWorkerBase( IAutoValidationController owner, Type interfaceType ) : base( owner, interfaceType ) {}

		public override object Invoke( MethodInvocationSingleArgumentParameter parameter ) => Owner.Execute( parameter );
	}

	class AutoValidationExecuteWorker : AutoValidationExecuteWorkerBase, IParameterHandlerAware
	{
		readonly CompositeParameterHandler handlers = new CompositeParameterHandler();

		public AutoValidationExecuteWorker( IAutoValidationController owner, Type interfaceType ) : base( owner, interfaceType ) {}

		public void Register( IParameterHandler handler ) => handlers.Add( handler );
		public bool Handles( object parameter ) => handlers.Handles( parameter );

		public object Handle( object parameter ) => handlers.Handle( parameter );

		/*public override object Invoke( MethodInvocationParameter parameter )
		{
			var handled = handlers.Count > 0 ? handlers.Handle( parameter.Arguments[0] ) : Placeholders.Null;
			var result = handled == Placeholders.Null ? base.Invoke( parameter ) : handled;
			return result;
		}*/
	}

	class GenericAutoValidationExecuteWorker : AutoValidationExecuteWorkerBase
	{
		readonly IRegistrationProvider provider;

		public GenericAutoValidationExecuteWorker( IAutoValidationController owner, Type interfaceType, Type registrationType ) 
			: this( owner, interfaceType, (IRegistrationProvider)SingletonLocator.Instance.Locate( registrationType ) ) {}

		GenericAutoValidationExecuteWorker( IAutoValidationController owner, Type interfaceType, IRegistrationProvider provider ) : base( owner, interfaceType )
		{
			this.provider = provider;
		}

		// protected override void OnInitialize() => Owner.Register( provider );

		public override Priority Priority => Priority.High;
	}

	/*[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public class RegisterHandlerAspect : ConnectionWorkerHostBase
	{
		public RegisterHandlerAspect( Type providerType ) : base( new WorkerFactory( providerType, ( controller, type ) => new RegisterHandlerWorker( controller, type ) ).Cached().ToDelegate() ) {}
	}

	class RegisterHandlerWorker : ConnectionWorkerBase<IAutoValidationController>
	{
		readonly IRegistrationProvider provider;

		public RegisterHandlerWorker( IAutoValidationController owner, Type registrationType ) : this( owner, (IRegistrationProvider)SingletonLocator.Instance.Locate( registrationType ) ) {}

		public RegisterHandlerWorker( IAutoValidationController owner, IRegistrationProvider provider ) : base( owner )
		{
			this.provider = provider;
		}

		protected override void OnInitialize() => Owner.Register( provider );

		public override Priority Priority => Priority.High;
	}*/

	class AdapterLocator : FactoryBase<object, IParameterValidationAdapter>
	{
		readonly ImmutableArray<IProfile> profiles;

		public static AdapterLocator Instance { get; } = new AdapterLocator();
		AdapterLocator() : this( AutoValidation.DefaultProfiles ) {}

		public AdapterLocator( ImmutableArray<IProfile> profiles )
		{
			this.profiles = profiles;
		}

		public override IParameterValidationAdapter Create( object parameter )
		{
			foreach ( var profile in profiles )
			{
				if ( profile.InterfaceType.IsInstanceOfTypeOrDefinition( parameter.GetType() ) )
				{
					return profile.AdapterFactory( parameter );
				}
			}
			return null;
		}
	}

	public interface IProfile : IEnumerable<Func<Type, AspectInstance>>
	{
		TypeAdapter InterfaceType { get; }

		Func<object, IParameterValidationAdapter> AdapterFactory { get; }
	}

	class Profile : IProfile
	{
		readonly Func<Type, AspectInstance> validate;
		readonly Func<Type, AspectInstance> execute;
		protected Profile( Type interfaceType, string valid, string execute, IFactory<object, IParameterValidationAdapter> factory ) : this( interfaceType.Adapt(), new AspectInstanceMethodFactory<AutoValidationValidationAspect>( interfaceType, valid ).ToDelegate(), new AspectInstanceMethodFactory<AutoValidationExecuteAspect>( interfaceType, execute ).ToDelegate(), factory.ToDelegate() ) {}

		protected Profile( TypeAdapter interfaceType, Func<Type, AspectInstance> validate, Func<Type, AspectInstance> execute, Func<object, IParameterValidationAdapter> adapterFactory )
		{
			InterfaceType = interfaceType;
			AdapterFactory = adapterFactory;
			this.validate = validate;
			this.execute = execute;
		}

		public TypeAdapter InterfaceType { get; }
		public Func<object, IParameterValidationAdapter> AdapterFactory { get; }

		public IEnumerator<Func<Type, AspectInstance>> GetEnumerator()
		{
			yield return validate;
			yield return execute;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class GenericProfile<T> : Profile
	{
		protected GenericProfile( Type interfaceType, string valid, string execute, IFactory<object, IParameterValidationAdapter> adapterFactory )
			: base( interfaceType.Adapt(),
					new GenericAspectInstanceMethodFactory<GenericAutoValidationValidationAspect, ValidationRegisterProvider<T>>( interfaceType, valid ).ToDelegate(),
					new GenericAspectInstanceMethodFactory<GenericAutoValidationExecuteAspect, ExecutionRegisterProvider<T>>( interfaceType, execute ).ToDelegate(),
					adapterFactory.ToDelegate()
				) {}
	}

	class AspectInstanceMethodFactory<T> : AspectInstanceFactoryBase where T : AutoValidationWorkerHostBase
	{
		public AspectInstanceMethodFactory( Type implementingType, string methodName ) : base( implementingType, methodName, Construct.Instance<T>( implementingType ) ) {}
	}

	class GenericAspectInstanceMethodFactory<TAspect, TRegistry> : AspectInstanceFactoryBase where TAspect : AutoValidationWorkerHostBase where TRegistry : RegisterProvider
	{
		public GenericAspectInstanceMethodFactory( Type implementingType, string methodName ) : base( implementingType, methodName, Construct.Instance<TAspect>( implementingType, typeof(TRegistry) ) ) {}
	}

	static class Construct
	{
		public static ObjectConstruction New<T>( params object[] arguments ) => new ObjectConstruction( typeof(T), arguments );

		// public static Func<MethodInfo, AspectInstance> Factory<T>() => HandlerFactory<T>.Instance.ToDelegate();

		public static Func<MethodInfo, AspectInstance> Instance<T>( params object[] arguments ) => new ConstructAspectInstanceFactory<T>( arguments ).ToDelegate();

		/*static class HandlerFactory<T>
		{
			public static ConstructAspectInstanceFactory<RegisterHandlerAspect> Instance { get; } = new ConstructAspectInstanceFactory<RegisterHandlerAspect>( typeof(T) );
		}*/
	}

	class ConstructAspectInstanceFactory<T> : FactoryBase<MethodInfo, AspectInstance>
	{
		readonly ObjectConstruction construction;

		public ConstructAspectInstanceFactory( params object[] arguments ) : this( Construct.New<T>( arguments ) ) {}

		ConstructAspectInstanceFactory( ObjectConstruction construction )
		{
			this.construction = construction;
		}

		public override AspectInstance Create( MethodInfo parameter ) => new AspectInstance( parameter, construction, null );
	}

	abstract class AspectInstanceFactoryBase : FactoryBase<Type, AspectInstance>
	{
		readonly Type implementingType;
		readonly string methodName;
		readonly Func<MethodInfo, AspectInstance> factory;

		protected AspectInstanceFactoryBase( Type implementingType, string methodName, Func<MethodInfo, AspectInstance> factory )
		{
			this.implementingType = implementingType;
			this.methodName = methodName;
			this.factory = factory;
		}

		public override AspectInstance Create( Type parameter )
		{
			var mappings = parameter.Adapt().GetMappedMethods( implementingType );
			var mapping = mappings.Introduce( methodName, pair => pair.Item1.Item1.Name == pair.Item2 && ( pair.Item1.Item2.IsFinal || pair.Item1.Item2.IsVirtual ) && !pair.Item1.Item2.IsAbstract ).SingleOrDefault();
			if ( mapping.IsAssigned() )
			{
				var method = mapping.Item2.AccountForGenericDefinition();
				var result = FromMethod( method );
				return result;
			}
			return null;
		}

		AspectInstance FromMethod( MethodInfo method )
		{
			var repository = PostSharpEnvironment.CurrentProject.GetService<IAspectRepositoryService>();
			var instance = factory( method );
			var type = instance.Aspect != null ? instance.Aspect.GetType() : Type.GetType( instance.AspectConstruction.TypeName );
			var result = !repository.HasAspect( method, type ) ? instance : null;
			return result;
		}
	}

	public interface IParameterHandler
	{
		bool Handles( object parameter );

		object Handle( object parameter );
	}

	class Specification<T> : SpecificationWithContextBase<IAutoValidationWorker, Type> where T : class, IAutoValidationWorker
	{
		public Specification( Type context ) : base( context ) {}

		public override bool IsSatisfiedBy( IAutoValidationWorker parameter )
		{
			var aspect = parameter as T;
			var result = aspect?.InterfaceType == Context;
			return result;
		}
	}

	public class RegisterProvider : DelegatedSpecification<IAutoValidationWorker>, IRegistrationProvider
	{
		readonly Func<IParameterValidationAdapter, IParameterHandler> create;

		public RegisterProvider( Func<IAutoValidationWorker, bool> specification, Func<IParameterValidationAdapter, IParameterHandler> create ) : base( specification )
		{
			this.create = create;
		}

		public IParameterHandler Create( IParameterValidationAdapter adapter ) => create( adapter );
	}

	class ValidationRegisterProvider<T> : RegisterProvider
	{
		public static ValidationRegisterProvider<T> Instance { get; } = new ValidationRegisterProvider<T>();

		ValidationRegisterProvider() : base( new Specification<AutoValidationValidateWorker>( typeof(T) ).ToDelegate(), adapter => new Handler( adapter ) ) {}

		class Handler : IParameterHandler
		{
			readonly IParameterValidationAdapter adapter;

			public Handler( IParameterValidationAdapter adapter )
			{
				this.adapter = adapter;
			}

			public bool Handles( object parameter ) => adapter.Handles( parameter );

			public object Handle( object parameter ) => adapter.IsValid( parameter );
		}
	}

	class ExecutionRegisterProvider<T> : RegisterProvider
	{
		public static ExecutionRegisterProvider<T> Instance { get; } = new ExecutionRegisterProvider<T>();

		ExecutionRegisterProvider() : base( new Specification<AutoValidationExecuteWorker>( typeof(T) ).ToDelegate(), adapter => new Handler( adapter ) ) {}

		class Handler : IParameterHandler
		{
			readonly IParameterValidationAdapter adapter;

			public Handler( IParameterValidationAdapter adapter )
			{
				this.adapter = adapter;
			}

			public bool Handles( object parameter ) => adapter.Handles( parameter );

			public object Handle( object parameter ) => adapter.Execute( parameter );
		}
	}

	class DefaultAspectInstanceFactory : AspectInstanceFactory
	{
		public static DefaultAspectInstanceFactory Instance { get; } = new DefaultAspectInstanceFactory();
		DefaultAspectInstanceFactory() : base( AutoValidation.DefaultProfiles ) {}
	}

	class GenericFactoryProfile : GenericProfile<IFactoryWithParameter>
	{
		public static GenericFactoryProfile Instance { get; } = new GenericFactoryProfile();
		GenericFactoryProfile() : base( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create), GenericFactoryAdapterFactory.Instance ) {}
	}

	class FactoryProfile : Profile
	{
		public static FactoryProfile Instance { get; } = new FactoryProfile();
		FactoryProfile() : base( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create), FactoryAdapterFactory.Instance ) {}
	}

	class GenericCommandProfile : GenericProfile<ICommand>
	{
		public static GenericCommandProfile Instance { get; } = new GenericCommandProfile();
		GenericCommandProfile() : base( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute), GenericCommandAdapterFactory.Instance ) {}
	}

	class CommandProfile : Profile
	{
		public static CommandProfile Instance { get; } = new CommandProfile();
		CommandProfile() : base( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute), CommandAdapterFactory.Instance ) {}
	}

	

	public class AspectInstanceFactory : FactoryBase<Type, IEnumerable<AspectInstance>>
	{
		readonly ImmutableArray<Func<Type, AspectInstance>> factories;

		public AspectInstanceFactory( ImmutableArray<IProfile> profiles ) : this( profiles.Select( profile => profile.InterfaceType ).ToImmutableArray(), profiles.ToArray().Concat().ToImmutableArray() ) {}

		AspectInstanceFactory( ImmutableArray<TypeAdapter> knownTypes, ImmutableArray<Func<Type, AspectInstance>> factories ) : this( new Specification( knownTypes ), factories ) {}

		AspectInstanceFactory( ISpecification<Type> specification, ImmutableArray<Func<Type, AspectInstance>> factories ) : base( specification )
		{
			this.factories = factories;
		}

		public override IEnumerable<AspectInstance> Create( Type parameter )
		{
			foreach ( var factory in factories )
			{
				var instance = factory( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}

		class Specification : SpecificationWithContextBase<Type, ImmutableArray<TypeAdapter>>
		{
			public Specification( ImmutableArray<TypeAdapter> context ) : base( context ) {}

			public override bool IsSatisfiedBy( Type parameter )
			{
				if ( !Context.IsAssignableFrom( parameter ) )
				{
					throw new InvalidOperationException( $"{parameter} does not implement any of the types defined in {GetType()}, which are: {string.Join( ",", Context.Select( t => t.Type.FullName ) )}" );
				}
				return true;
			}
		}
	}	

	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	// [MulticastAttributeUsage( TargetMemberAttributes = MulticastAttributes.NonAbstract | MulticastAttributes.Instance )]
	public class ApplyAutoValidationAttribute : ConnectionOwnerHostBase, IAspectProvider
	{
		readonly IFactory<Type, IEnumerable<AspectInstance>> provider;

		public ApplyAutoValidationAttribute() : this( Services.Controller.ToDelegate(), DefaultAspectInstanceFactory.Instance ) {}

		protected ApplyAutoValidationAttribute( Func<object, IAutoValidationController> factory, IFactory<Type, IEnumerable<AspectInstance>> provider ) : base( factory )
		{
			this.provider = provider;
		}

		public override bool CompileTimeValidate( Type type ) => provider.CanCreate( type );

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement )
		{
			var type = targetElement as Type;
			var result = type != null ? provider.Create( type ) : Items<AspectInstance>.Default;
			return result;
		}
	}
}