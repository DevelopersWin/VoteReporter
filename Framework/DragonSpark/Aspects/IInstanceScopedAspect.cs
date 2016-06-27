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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DragonSpark.Aspects
{
	public interface IInstanceScopedAspect : PostSharp.Aspects.IInstanceScopedAspect
	{
		void RuntimeInitializeInstance( object instance );
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
				var returnValue = handled == Null ? Execute( new AutoValidationParameter( args, parameter ) ) : handled;
				args.ReturnValue = returnValue ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		protected abstract object Execute( AutoValidationParameter parameter );
	}

	/*class RegisterFactoryValidationHandlerCommand : RegisterValidationHandlerCommand<IFactoryWithParameter>
	{
		public static RegisterFactoryValidationHandlerCommand Instance { get; } = new RegisterFactoryValidationHandlerCommand();
		RegisterFactoryValidationHandlerCommand() {}
	}

	class RegisterCommandValidationHandlerCommand : RegisterValidationHandlerCommand<ICommand>
	{
		public static RegisterCommandValidationHandlerCommand Instance { get; } = new RegisterCommandValidationHandlerCommand();
		RegisterCommandValidationHandlerCommand() {}
	}*/

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
		protected static object Null { get; } = new object();

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
			return Null;
		}
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

	public static class Services
	{
		public static ICache<IList<IParameterHandler>> Handlers { get; } = new ListCache<IParameterHandler>();
		public static ICache<InstanceAwareRepository> Instances { get; } = new ActivatedCache<InstanceAwareRepository>();
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

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	/*[AspectTypeDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(AutoValidationValidateAspect) )]
	[AspectTypeDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(AutoValidationExecuteAspect) )]*/
	[MulticastAttributeUsage( TargetMemberAttributes = MulticastAttributes.NonAbstract | MulticastAttributes.Instance )]
	public class ApplyAutoValidationAttribute : InstanceAwareParentAspectBase, IAspectProvider
	{
		readonly Func<object, IParameterValidationAdapter> factory;
		
		public ApplyAutoValidationAttribute() : this( AdapterLocator.Instance.ToDelegate() ) {}

		protected ApplyAutoValidationAttribute( Func<object, IParameterValidationAdapter> factory, [Optional]IFactory<Type, IEnumerable<AspectInstance>> provider )
		{
			this.factory = factory;
			this.provider = provider;
		}

		IFactory<Type, IEnumerable<AspectInstance>> Provider => provider ?? ( provider = new DefaultAspectInstanceFactory() );
		IFactory<Type, IEnumerable<AspectInstance>> provider;
		public override bool CompileTimeValidate( Type type ) => Provider.CanCreate( type );

		public override void RuntimeInitializeInstance()
		{
			var adapter = AutoValidation.Adapter.SetValue( Instance, factory( Instance ) );
			AutoValidation.Controller.Set( Instance, new AutoValidationController( adapter ) );
			base.RuntimeInitializeInstance();
		}

		class AdapterLocator : FactoryBase<object, IParameterValidationAdapter>
		{
			public static ImmutableArray<Type> KnownTypes { get; } = ImmutableArray.Create( typeof(IFactory<,>), typeof(IFactoryWithParameter), typeof(ICommand<>), typeof(ICommand) );

			public static AdapterLocator Instance { get; } = new AdapterLocator();

			readonly static ImmutableArray<ValueTuple<TypeAdapter, Func<object, IParameterValidationAdapter>>> Adapters = KnownTypes.Select( type => type.Adapt() ).ToArray().Tuple( new IFactory<object, IParameterValidationAdapter>[] { GenericFactoryAdapterFactory.Instance, FactoryAdapterFactory.Instance, GenericCommandAdapterFactory.Instance, CommandAdapterFactory.Instance }.Select( x => x.ToDelegate() ) ).ToImmutableArray();

			public override IParameterValidationAdapter Create( object parameter )
			{
				foreach ( var tuple in Adapters )
				{
					if ( tuple.Item1.IsInstanceOfTypeOrDefinition( parameter ) )
					{
						return tuple.Item2( parameter );
					}
				}
				return null;
			}
		}

		class DefaultAspectInstanceFactory : AspectInstanceFactory
		{
			public DefaultAspectInstanceFactory() : base( 
				AdapterLocator.KnownTypes,
				Factory.GenericValidation,
				Factory.Validation
				// new ExecutionAspectInstanceFactory( typeof(IFactory<,>), nameof(IFactoryWithParameter.Create) ).ToDelegate()
				// new ExecutionAspectInstanceFactory( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.Create) ).ToDelegate(),
				
				) {}

			static class Factory
			{
				public static Func<Type, ImmutableArray<AspectInstance>> Validation { get; } = new AspectInstanceMethodFactory<AutoValidationValidateAspect>( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate) ).ToDelegate();
				public static Func<Type, ImmutableArray<AspectInstance>> GenericValidation { get; } = new GenericAspectInstanceMethodFactory<AutoValidationValidateAspect, RegisterValidationHandlerCommand<IFactoryWithParameter>>( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate) ).ToDelegate();
			}
		}

		public class AspectInstanceFactory : FactoryBase<Type, IEnumerable<AspectInstance>>
		{
			readonly Func<Type, ImmutableArray<AspectInstance>>[] factories;

			public AspectInstanceFactory( ImmutableArray<Type> knownTypes, params Func<Type, ImmutableArray<AspectInstance>>[] factories ) : this( new Specification( knownTypes ), factories ) {}

			AspectInstanceFactory( ISpecification<Type> specification, params Func<Type, ImmutableArray<AspectInstance>>[] factories ) : base( specification )
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

		public static class Construct
		{
			public static ObjectConstruction New<T>( params object[] arguments ) => new ObjectConstruction( typeof(T), arguments );
		}

		static class RegisterHandlerFactory<T>
		{
			public static ConstructAspectInstanceFactory<RegisterHandlerAspect> Instance { get; } = new ConstructAspectInstanceFactory<RegisterHandlerAspect>( typeof(T) );
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
			public AspectInstanceMethodFactory( Type implementingType, string methodName ) : base( implementingType, methodName, new ConstructAspectInstanceFactory<T>( implementingType ).ToDelegate() ) {}
		}

		class GenericAspectInstanceMethodFactory<TAspect, TCommand> : AspectInstanceFactoryBase where TAspect : IAutoValidationAspect where TCommand : RegisterHandlerCommand
		{
			public GenericAspectInstanceMethodFactory( Type implementingType, string methodName ) : base( implementingType, methodName, RegisterHandlerFactory<TCommand>.Instance.ToDelegate(), new ConstructAspectInstanceFactory<TAspect>( implementingType ).ToDelegate() ) {}
		}

		abstract class AspectInstanceFactoryBase : FactoryBase<Type, ImmutableArray<AspectInstance>>
		{
			readonly Type implementingType;
			readonly string methodName;
			readonly Func<MethodInfo, AspectInstance>[] factories;

			protected AspectInstanceFactoryBase( Type implementingType, string methodName, params Func<MethodInfo, AspectInstance>[] factories )
			{
				this.implementingType = implementingType;
				this.methodName = methodName;
				this.factories = factories;
			}

			public override ImmutableArray<AspectInstance> Create( Type parameter )
			{
				var mappings = parameter.Adapt().GetMappedMethods( implementingType );
				var mapping = mappings.Introduce( methodName, pair => pair.Item1.Item1.Name == pair.Item2 && ( pair.Item1.Item2.IsFinal || pair.Item1.Item2.IsVirtual ) && !pair.Item1.Item2.IsAbstract ).SingleOrDefault();
				var method = mapping.Item2.AccountForGenericDefinition();
				var result = FromMethod( method ).ToImmutableArray();
				return result;
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
						// MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"YO: {instance.AspectConstruction.ConstructorArguments[0]}", null, null, null ));
						yield return instance;
					}
				}
			}
		}

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement )
		{
			var type = targetElement as Type;
			var result = type != null ? Provider.Create( type ) : Items<AspectInstance>.Default;
			return result;
		}
	}
}