namespace DragonSpark.Aspects.Validation
{
	/*public interface IParameterHandlerAware : IParameterHandler
	{
		void Register( IParameterHandler handler );
	}

	public interface IConnectionOwner : IConnectionAware
	{
		void Connect( IConnectionWorker worker );
	}

	public interface IConnectionAware
	{
		void Initialize();
	}

	public abstract class ConnectionAwareBase : IConnectionAware
	{
		public void Initialize() => OnInitialize();

		protected virtual void OnInitialize() {}
	}

	public class CompositeParameterHandler : List<IParameterHandler>, IParameterHandler
	{
		public bool Handles( object parameter )
		{
			foreach ( var handler in this )
			{
				if ( handler.Handles( parameter ) )
				{
					return true;
				}
			}
			return false;
		}

		public object Handle( object parameter )
		{
			foreach ( var handler in this )
			{
				if ( handler.Handles( parameter ) )
				{
					return handler.Handle( parameter );
				}
			}
			return Placeholders.Null;
		}
	}

	public abstract class ConnectionOwnerBase : ConnectionAwareBase, IConnectionOwner
	{
		readonly List<IConnectionWorker> workers = new List<IConnectionWorker>();

		protected ConnectionOwnerBase()
		{
			Workers = workers;
		}

		public IEnumerable<IConnectionWorker> Workers { get; } // TODO: Address this.

		protected override void OnInitialize()
		{
			// workers.Sort( PriorityComparer.Instance );
			foreach ( var worker in workers )
			{
				worker.Initialize();
			}
			// workers.Clear();
		}

		public void Connect( IConnectionWorker worker ) => workers.Add( worker );
	}

	public interface IConnectionWorker : IConnectionAware, IPriorityAware {}

	public abstract class ConnectionAwareBase<T> : ConnectionAwareBase where T : IConnectionOwner
	{
		protected ConnectionAwareBase( T owner )
		{
			Owner = owner;
		}

		protected T Owner { get; }

		public virtual Priority Priority => Priority.Normal;
	}

	public abstract class ConnectionWorkerBase<T> : ConnectionAwareBase<T>, IConnectionWorker where T : IConnectionOwner
	{
		protected ConnectionWorkerBase( T owner ) : base( owner )
		{
			owner.Connect( this );
		}
	}*/

	/*public class MethodInvocationParameterPool : PoolableBuilderBase<MethodInvocationParameter>
	{
		public static MethodInvocationParameterPool Instance { get; } = new MethodInvocationParameterPool();

		protected override void Apply( MethodInvocationParameter parameter, object instance, MethodBase method, Arguments arguments, Func<object> proceed ) 
			=> parameter.Apply( instance, method, arguments.ToArray(), proceed );
	}

	public class MethodInvocationSingleParameterPool : PoolableBuilderBase<MethodInvocationSingleParameter>
	{
		public static MethodInvocationSingleParameterPool Instance { get; } = new MethodInvocationSingleParameterPool();

		protected override void Apply( MethodInvocationSingleParameter parameter, object instance, MethodBase method, Arguments arguments, Func<object> proceed ) 
			=> parameter.Apply( instance, method, arguments?[0], proceed );
	}

	public abstract class PoolableBuilderBase<T> where T : class, IMethodInvocationParameter, new()
	{
		readonly ObjectPool<T> pool;

		protected PoolableBuilderBase() : this( new PoolStore().Value ) {}

		protected PoolableBuilderBase( ObjectPool<T> pool )
		{
			this.pool = pool;
		}

		protected class PoolStore : FixedStore<ObjectPool<T>>
		{
			public PoolStore( int size = 128 )
			{
				Assign( new ObjectPool<T>( Create, size ) );
			}

			protected virtual T Create() => new T();
		}

		public PooledContext From( object instance, MethodBase method, Arguments arguments, Func<object> proceed )
		{
			var item = pool.Allocate();
			Apply( item, instance, method, arguments, proceed );
			var result = new PooledContext( this, item );
			return result;
		}

		protected abstract void Apply( T parameter, object instance, MethodBase method, Arguments arguments, Func<object> proceed );

		public virtual void Free( T item )
		{
			// item.Clear();
			pool.Free( item );
		}


		public struct PooledContext : IDisposable
		{
			readonly PoolableBuilderBase<T> owner;

			public PooledContext( PoolableBuilderBase<T> owner, T item )
			{
				this.owner = owner;
				Item = item;
			}

			public T Item { get; }

			public void Dispose() => owner.Free( Item );
		}
	}*/

	/*public struct MethodInvocationSingleArgumentParameter
	{
		public MethodInvocationSingleArgumentParameter( object instance, MethodBase method, object argument, Func<object> proceed )
		{
			Instance = instance;
			Method = method;
			Argument = argument;
			Proceed = proceed;
		}

		public object Instance { get; }
		public MethodBase Method { get; }
		public object Argument { get; }
		public Func<object> Proceed { get; }
	}

	public struct MethodInvocationParameter
	{
		public MethodInvocationParameter( object instance, MethodBase method, object[] arguments, Func<object> proceed )
		{
			Instance = instance;
			Method = method;
			Arguments = arguments;
			Proceed = proceed;
		}

		public object Instance { get; }
		public MethodBase Method { get; }
		public object[] Arguments { get; }
		public Func<object> Proceed { get; }
	}*/

	/*public class MethodInvocationSingleParameter : MethodInvocationParameterBase<object> {}

	public class MethodInvocationParameter : MethodInvocationParameterBase<object[]>
	{
		public MethodInvocationParameter() {}
	}*/

	/*public interface IMethodInvocationParameter
	{
		object Instance { get; }
		MethodBase Method { get; }
		object Argument { get; }
		Func<object> Proceed {get; }

		void Clear();
	}*/

	/*public abstract class MethodInvocationParameterBase<T> : IMethodInvocationParameter
	{
		public void Apply( object instance, MethodBase method, T argument, Func<object> proceed )
		{
			Instance = instance;
			Method = method;
			Argument = argument;
			Proceed = proceed;
		}
		

		public object Instance { get; private set; }
		public MethodBase Method { get; private set; }
		public T Argument { get; private set; }
		public Func<object> Proceed {get; private set; }

		public void Clear()
		{
			Instance = null;
			Method = null;
			Argument = default(T);
			Proceed = null;
		}

		object IMethodInvocationParameter.Argument => Argument;
	}*/

	/*public interface IConnectionWorkerHost : IWritableStore<IConnectionWorker> {}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class ConnectionOwnerHostBase : InstanceLevelAspect
	{
		readonly Func<object, IConnectionOwner> factory;

		protected ConnectionOwnerHostBase( Func<object, IConnectionOwner> factory )
		{
			this.factory = factory;
		}

		public override void RuntimeInitializeInstance() => factory( Instance ).Initialize();
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class ConnectionWorkerHostBase : MethodInterceptionAspect, IInstanceScopedAspect, IConnectionWorkerHost
	{
		readonly Func<object, IConnectionWorker> worker;
		
		protected ConnectionWorkerHostBase( Func<object, IConnectionWorker> worker )
		{
			this.worker = worker;
		}

		public object CreateInstance( AdviceArgs adviceArgs )
		{
			var result = (IConnectionWorkerHost)MemberwiseClone();
			var instance = worker( adviceArgs.Instance );
			result.Assign( instance );
			return result;
		}

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
		public void Assign( IConnectionWorker item ) => Value = item;

		public IConnectionWorker Value { get; private set; }

		object IStore.Value => Value;

		void IWritableStore.Assign( object item ) => Value = (IConnectionWorker)item;
	}*/


	/*public static class Services
	{
		public static ICache<IAutoValidationController> Controller { get; } = new Cache<IAutoValidationController>( o => new AutoValidationController( AdapterLocator.Instance.Create( o ) ) );
		// public static ICache<IParameterValidationAdapter> Adapter { get; } = AdapterLocator.Instance.Cached().ToDelegate();

		/*public static ICache<IList<IParameterHandler>> Handlers { get; } = new ListCache<IParameterHandler>();
		public static ICache<InstanceAwareRepository> Instances { get; } = new ActivatedCache<InstanceAwareRepository>();#1#
	}*/

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

	/*public interface IAutoValidationWorker : IConnectionWorker
	{
		Type InterfaceType { get; }

		object Invoke( MethodInvocationSingleArgumentParameter parameter );
	}*/

	/*public interface IAutoValidationController //: IConnectionOwner, IParameterHandlerAware
	{
		bool IsValid( object parameter );

		void MarkValid( object parameter, bool valid );

		object Execute( object parameter );

		// void Register( IRegistrationProvider provider );
	}

	public class AutoValidationController : /*ConnectionOwnerBase,#1# IAutoValidationController
	{
		// readonly CompositeParameterHandler handlers = new CompositeParameterHandler();
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

		/*public void Register( IRegistrationProvider provider )
		{
			/*foreach ( var worker in Workers )
			{
				if ( provider.IsSatisfiedBy( worker ) )
				{
					var handler = provider.Create( adapter );
					((IParameterHandlerAware)worker).Register( handler );
					break;
				}
			}#2#
		}#1#

		/*public void Register( IParameterHandler handler ) => handlers.Add( handler );
		public bool Handles( object parameter ) => handlers.Handles( parameter );

		public object Handle( object parameter ) => handlers.Handle( parameter );#1#
	}*/

	/*public interface IRegistrationProvider : ISpecification<IAutoValidationWorker>
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
		}#1#
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
		}#1#
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
	}#1#

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
	}*/
}