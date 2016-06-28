using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	public static class Services
	{
		public static ICache<IList<IParameterHandler>> Handlers { get; } = new ListCache<IParameterHandler>();
		public static ICache<InstanceAwareRepository> Instances { get; } = new ActivatedCache<InstanceAwareRepository>();
	}

	public interface IInstanceScopedAspect : PostSharp.Aspects.IInstanceScopedAspect
	{
		void RuntimeInitializeInstance( object instance );
	}
	

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
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
	}

	public interface IAutoValidationAspect : IInstanceScopedAspect
	{
		Type InterfaceType { get; }
	}

	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class AutoValidationAspectBase : HandlerAwareAspectBase, IAutoValidationAspect
	{
		readonly static Func<object, IAutoValidationController> Get = AutoValidation.Controller.Get;

		protected AutoValidationAspectBase( Type interfaceType )
		{
			InterfaceType = interfaceType;
		}

		public Type InterfaceType { get; }

		public override void RuntimeInitializeInstance( object instance )
		{
			Controller = Get( instance );
			base.RuntimeInitializeInstance( instance );
		}

		protected IAutoValidationController Controller { get; private set; }

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( Controller != null )
			{
				var parameter = args.Arguments[0];
				var handled = Handle( parameter );
				var returnValue = handled == Placeholders.Null ? Execute( new AutoValidationParameter( args, parameter ) ) : handled;
				args.ReturnValue = returnValue ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		protected abstract object Execute( AutoValidationParameter parameter );
	}

	class RegisterValidationHandlerCommand<T> : RegisterHandlerCommand
	{
		readonly static ICache<IParameterHandler> Cache = new Cache<IParameterHandler>( o => new Handler( AutoValidation.Adapter.Get( o ) ) );

		public static RegisterValidationHandlerCommand<T> Instance { get; } = new RegisterValidationHandlerCommand<T>();

		RegisterValidationHandlerCommand() : base( Cache, new Specification<AutoValidationValidateAspect>( typeof(T) ).ToDelegate() ) {}

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

	class RegisterExecutionHandlerCommand<T> : RegisterHandlerCommand
	{
		readonly static ICache<IParameterHandler> Cache = new Cache<IParameterHandler>( o => new Handler( AutoValidation.Adapter.Get( o ) ) );

		public static RegisterExecutionHandlerCommand<T> Instance { get; } = new RegisterExecutionHandlerCommand<T>();

		RegisterExecutionHandlerCommand() : base( Cache, new Specification<AutoValidationExecuteAspect>( typeof(T) ).ToDelegate() ) {}

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

	class Specification<T> : SpecificationWithContextBase<IAutoValidationAspect, Type> where T : class, IAutoValidationAspect
	{
		public Specification( Type context ) : base( context ) {}

		public override bool IsSatisfiedBy( IAutoValidationAspect parameter )
		{
			var aspect = parameter as T;
			var result = aspect?.InterfaceType == Context;
			return result;
		}
	}

	public class RegisterHandlerCommand : CommandBase<object>
	{
		readonly ICache<IParameterHandler> handler;
		readonly Func<IAutoValidationAspect, bool> specification;

		public RegisterHandlerCommand( ICache<IParameterHandler> handler, Func<IAutoValidationAspect, bool> specification )
		{
			this.handler = handler;
			this.specification = specification;
		}

		public override void Execute( object parameter )
		{
			foreach ( var aspect in Services.Instances.Get( parameter ).List() )
			{
				var instance = aspect as IAutoValidationAspect;
				if ( instance != null && specification( instance ) )
				{
					Services.Handlers.Get( aspect ).Add( handler.Get( parameter ) );
				}
			}
		}
	}

	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public class RegisterHandlerAspect : InstanceAwareChildAspectBase, IPriorityAware
	{
		readonly RegisterHandlerCommand command;

		public RegisterHandlerAspect( Type registerHandlerCommandType ) : this( (RegisterHandlerCommand)SingletonLocator.Instance.Locate( registerHandlerCommandType ) ) {}

		RegisterHandlerAspect( RegisterHandlerCommand command )
		{
			this.command = command;
		}

		public override void RuntimeInitializeInstance( object instance ) => command.Execute( instance );

		public Priority Priority => Priority.High;
	}

	public class AutoValidationValidateAspect : AutoValidationAspectBase
	{
		public AutoValidationValidateAspect( Type interfaceType ) : base( interfaceType ) {}

		protected override object Execute( AutoValidationParameter parameter )
		{
			var result = parameter.Proceed<bool>();
			Controller.MarkValid( parameter.Parameter, result );
			return null;
		}
	}

	public class AutoValidationExecuteAspect : AutoValidationAspectBase
	{
		public AutoValidationExecuteAspect( Type interfaceType ) : base( interfaceType ) {}

		protected override object Execute( AutoValidationParameter parameter ) => Controller.Execute( parameter );
	}

	public abstract class HandlerAwareAspectBase : InstanceAwareChildAspectBase
	{
		ImmutableArray<IParameterHandler> Handlers { get; set; }

		public override void RuntimeInitializeInstance( object instance ) => Handlers = Services.Handlers.Get( instance ).Concat( Services.Handlers.Get( this ) ).ToImmutableArray();

		protected object Handle( object parameter )
		{
			foreach ( var handler in Handlers )
			{
				if ( handler.Handles( parameter ) )
				{
					return handler.Handle( parameter );
				}
			}
			return Placeholders.Null;
		}
	}

	public class InstanceAwareRepository : RepositoryBase<IInstanceScopedAspect>, IDisposable
	{
		~InstanceAwareRepository()
		{
			Store.Clear();
		}

		[Freeze]
		public override ImmutableArray<IInstanceScopedAspect> List() => base.List();

		public void Dispose()
		{
			Store.Clear();
			GC.SuppressFinalize( this );
		}
	}

	public interface IParameterHandler
	{
		bool Handles( object parameter );

		object Handle( object parameter );
	}

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
				if ( profile.InterfaceType.Adapt().IsInstanceOfTypeOrDefinition( parameter ) )
				{
					return profile.AdapterFactory( parameter );
				}
			}
			return null;
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

	public interface IProfile : IEnumerable<Func<Type, ImmutableArray<AspectInstance>>>
	{
		Type InterfaceType { get; }

		Func<object, IParameterValidationAdapter> AdapterFactory { get; }
	}

	class Profile : IProfile
	{
		readonly Func<Type, ImmutableArray<AspectInstance>> validate;
		readonly Func<Type, ImmutableArray<AspectInstance>> execute;
		protected Profile( Type interfaceType, string valid, string execute, IFactory<object, IParameterValidationAdapter> factory ) : this( interfaceType, new AspectInstanceMethodFactory<AutoValidationValidateAspect>( interfaceType, valid ).ToDelegate(), new AspectInstanceMethodFactory<AutoValidationExecuteAspect>( interfaceType, execute ).ToDelegate(), factory.ToDelegate() ) {}

		protected Profile( Type interfaceType, Func<Type, ImmutableArray<AspectInstance>> validate, Func<Type, ImmutableArray<AspectInstance>> execute, Func<object, IParameterValidationAdapter> adapterFactory )
		{
			InterfaceType = interfaceType;
			AdapterFactory = adapterFactory;
			this.validate = validate;
			this.execute = execute;
		}

		public Type InterfaceType { get; }
		public Func<object, IParameterValidationAdapter> AdapterFactory { get; }

		public IEnumerator<Func<Type, ImmutableArray<AspectInstance>>> GetEnumerator()
		{
			yield return validate;
			yield return execute;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class GenericProfile<T> : Profile
	{
		protected GenericProfile( Type interfaceType, string valid, string execute, IFactory<object, IParameterValidationAdapter> adapterFactory )
			: base( interfaceType,
					new GenericAspectInstanceMethodFactory<AutoValidationValidateAspect, RegisterValidationHandlerCommand<T>>( interfaceType, valid ).ToDelegate(),
					new GenericAspectInstanceMethodFactory<AutoValidationExecuteAspect, RegisterExecutionHandlerCommand<T>>( interfaceType, execute ).ToDelegate(),
					adapterFactory.ToDelegate()
				) {}
	}

	public class AspectInstanceFactory : FactoryBase<Type, IEnumerable<AspectInstance>>
	{
		readonly ImmutableArray<Func<Type, ImmutableArray<AspectInstance>>> factories;

		public AspectInstanceFactory( ImmutableArray<IProfile> profiles ) : this( profiles.Select( profile => profile.InterfaceType ).ToImmutableArray(), profiles.ToArray().Concat().ToImmutableArray() ) {}

		AspectInstanceFactory( ImmutableArray<Type> knownTypes, ImmutableArray<Func<Type, ImmutableArray<AspectInstance>>> factories ) : this( new Specification( knownTypes ), factories ) {}

		AspectInstanceFactory( ISpecification<Type> specification, ImmutableArray<Func<Type, ImmutableArray<AspectInstance>>> factories ) : base( specification )
		{
			this.factories = factories;
		}

		public override IEnumerable<AspectInstance> Create( Type parameter )
		{
			foreach ( var factory in factories )
			{
				foreach ( var instance in factory( parameter ) )
				{
					yield return instance;
				}
			}
		}

		class Specification : SpecificationWithContextBase<Type, ImmutableArray<TypeAdapter>>
		{
			public Specification( ImmutableArray<Type> context ) : base( context.Select( type => type.Adapt() ).ToImmutableArray() ) {}

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

	static class Construct
	{
		public static ObjectConstruction New<T>( params object[] arguments ) => new ObjectConstruction( typeof(T), arguments );

		public static Func<MethodInfo, AspectInstance> Factory<T>() => HandlerFactory<T>.Instance.ToDelegate();

		public static Func<MethodInfo, AspectInstance> Instance<T>( Type interfaceType ) => new ConstructAspectInstanceFactory<T>( interfaceType ).ToDelegate();

		static class HandlerFactory<T>
		{
			public static ConstructAspectInstanceFactory<RegisterHandlerAspect> Instance { get; } = new ConstructAspectInstanceFactory<RegisterHandlerAspect>( typeof(T) );
		}
	}

	class ConstructAspectInstanceFactory<T> : FactoryBase<MethodInfo, AspectInstance>
	{
		readonly ObjectConstruction construction;

		public ConstructAspectInstanceFactory( Type interfaceType ) : this( Construct.New<T>( interfaceType ) ) {}

		ConstructAspectInstanceFactory( ObjectConstruction construction )
		{
			this.construction = construction;
		}

		public override AspectInstance Create( MethodInfo parameter ) => new AspectInstance( parameter, construction, null );
	}

	class AspectInstanceMethodFactory<T> : AspectInstanceFactoryBase where T : IAutoValidationAspect
	{
		public AspectInstanceMethodFactory( Type implementingType, string methodName ) : base( implementingType, methodName, Construct.Instance<T>( implementingType ) ) {}
	}

	class GenericAspectInstanceMethodFactory<TAspect, TCommand> : AspectInstanceFactoryBase where TAspect : IAutoValidationAspect where TCommand : RegisterHandlerCommand
	{
		public GenericAspectInstanceMethodFactory( Type implementingType, string methodName ) : base( implementingType, methodName, Construct.Factory<TCommand>(), Construct.Instance<TAspect>( implementingType ) ) {}
	}

	abstract class AspectInstanceFactoryBase : FactoryBase<Type, ImmutableArray<AspectInstance>>
	{
		readonly Type implementingType;
		readonly string methodName;
		readonly ImmutableArray<Func<MethodInfo, AspectInstance>> factories;

		protected AspectInstanceFactoryBase( Type implementingType, string methodName, params Func<MethodInfo, AspectInstance>[] factories )
		{
			this.implementingType = implementingType;
			this.methodName = methodName;
			this.factories = factories.ToImmutableArray();
		}

		public override ImmutableArray<AspectInstance> Create( Type parameter )
		{
			var mappings = parameter.Adapt().GetMappedMethods( implementingType );
			var mapping = mappings.Introduce( methodName, pair => pair.Item1.Item1.Name == pair.Item2 && ( pair.Item1.Item2.IsFinal || pair.Item1.Item2.IsVirtual ) && !pair.Item1.Item2.IsAbstract ).SingleOrDefault();
			if ( mapping.IsAssigned() )
			{
				var method = mapping.Item2.AccountForGenericDefinition();
				var result = FromMethod( method ).ToImmutableArray();
				return result;
			}
			return ImmutableArray<AspectInstance>.Empty;
		}

		IEnumerable<AspectInstance> FromMethod( MethodInfo method )
		{
			var repository = PostSharpEnvironment.CurrentProject.GetService<IAspectRepositoryService>();
			foreach ( var factory in factories )
			{
				var instance = factory( method );
				var type = instance.Aspect != null ? instance.Aspect.GetType() : Type.GetType( instance.AspectConstruction.TypeName );
				if ( !repository.HasAspect( method, type ) )
				{
					yield return instance;
				}
			}
		}
	}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	// [MulticastAttributeUsage( TargetMemberAttributes = MulticastAttributes.NonAbstract | MulticastAttributes.Instance )]
	public class ApplyAutoValidationAttribute : InstanceAwareParentAspectBase, IAspectProvider
	{
		readonly Func<object, IParameterValidationAdapter> factory;
		readonly IFactory<Type, IEnumerable<AspectInstance>> provider;

		public ApplyAutoValidationAttribute() : this( AdapterLocator.Instance.ToDelegate(), DefaultAspectInstanceFactory.Instance ) {}

		protected ApplyAutoValidationAttribute( Func<object, IParameterValidationAdapter> factory, IFactory<Type, IEnumerable<AspectInstance>> provider )
		{
			this.factory = factory;
			this.provider = provider;
		}

		public override bool CompileTimeValidate( Type type ) => provider.CanCreate( type );

		public override void RuntimeInitializeInstance()
		{
			var adapter = AutoValidation.Adapter.SetValue( Instance, factory( Instance ) );
			AutoValidation.Controller.Set( Instance, new AutoValidationController( adapter ) );
			base.RuntimeInitializeInstance();
		}

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement )
		{
			var type = targetElement as Type;
			var result = type != null ? provider.Create( type ) : Items<AspectInstance>.Default;
			return result;
		}
	}
}