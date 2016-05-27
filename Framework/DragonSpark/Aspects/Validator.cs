using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
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

	public static class Properties
	{
		public static IAttachedProperty<InstanceServiceProvider> Services = new ActivatedAttachedProperty<InstanceServiceProvider>();
	}

	public delegate bool IsValid( object parameter );

	public delegate object Execute( object parameter );

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

	abstract class ParameterStoreBase<T> : AssignedAttachedPropertyStore<object, ParameterRelay>
	{
		protected override ParameterRelay CreateValue( object instance ) => new ParameterRelay( CreateFrom( instance ), new StateProvider( instance ) );

		 IParameterAware CreateFrom( object instance ) => instance is T ? Project( (T)instance ) : null;

		protected abstract IParameterAware Project( T instance );
	}

	class WorkflowState : ThreadLocalAttachedProperty<IParameterWorkflowState>
	{
		public static WorkflowState Property { get; } = new WorkflowState();

		WorkflowState() : base( () => new ParameterWorkflowState() ) {}
	}

	abstract class GenericParameterStoreBase : ParameterStoreBase<object>
	{
		readonly Type genericType;
		readonly string methodName;
		readonly TypeAdapter adapter;

		protected GenericParameterStoreBase( Type genericType, string methodName = nameof(Create) )
		{
			this.genericType = genericType;
			this.methodName = methodName;
			adapter = GetType().Adapt();
		}

		protected override IParameterAware Project( object instance )
		{
			var arguments = instance.GetType().Adapt().GetTypeArgumentsFor( genericType );
			var result = adapter.Invoke<IParameterAware>( methodName, arguments, instance );
			return result;
		}
	}

	class GenericFactoryParameterStore : GenericParameterStoreBase
	{
		public new static GenericFactoryParameterStore Instance { get; } = new GenericFactoryParameterStore();

		GenericFactoryParameterStore() : base( typeof(IFactory<,>), nameof(Create) ) {}

		static IParameterAware Create<TParameter, TResult>( IFactory<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}
	class GenericCommandParameterStore : GenericParameterStoreBase
	{
		public new static GenericCommandParameterStore Instance { get; } = new GenericCommandParameterStore();

		GenericCommandParameterStore() : base( typeof(ICommand<>), nameof(Create) ) {}

		static IParameterAware Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
	}

	class CommandParameterStore : ParameterStoreBase<ICommand>
	{
		public new static CommandParameterStore Instance { get; } = new CommandParameterStore();

		protected override IParameterAware Project( ICommand instance ) => new CommandAdapter( instance );
	}

	class FactoryParameterStore : ParameterStoreBase<IFactoryWithParameter>
	{
		public new static FactoryParameterStore Instance { get; } = new FactoryParameterStore();

		protected override IParameterAware Project( IFactoryWithParameter instance ) => new DelegatedParameterAware( instance.CanCreate, instance.Create );
	}

	/*public interface IStateProvider
	{
		IParameterWorkflowState Get();
	}*/

	public struct StateProvider
	{
		readonly object instance;

		public StateProvider( object instance )
		{
			this.instance = instance;
		}

		// [Pure]
		public IParameterWorkflowState Get() => WorkflowState.Property.Get( instance );
	}

	public struct ParameterRelay
	{
		readonly IParameterAware adapter;
		readonly StateProvider state;

		public ParameterRelay( IParameterAware adapter, StateProvider state )
		{
			this.adapter = adapter;
			this.state = state;
		}

		// [RecursionGuard()]
		public bool IsValid( RelayParameter<bool> parameter )
		{
			var workflow = new ParameterWorkflow( state.Get(), new ParameterWorkflowContext( adapter, parameter ) );
			var result = workflow.IsValid( parameter.Parameter );
			return result;
		}

		// [RecursionGuard()]
		public object Execute( RelayParameter<object> parameter )
		{
			var workflow = new ParameterWorkflow( state.Get(), new ParameterWorkflowContext( adapter, execute: parameter ) );
			var result = workflow.Execute( parameter.Parameter );
			return result;
		}
	}

	public struct RelayParameter<T>
	{
		public RelayParameter( Func<T> factory, object parameter )
		{
			Factory = factory;
			Parameter = parameter;
		}

		public Func<T> Factory { get; }
		public object Parameter { get; }
	}
	
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ParameterValidatorBase : TypeLevelAspect
	{
		readonly Profile profile;
		readonly Func<object, ParameterRelay> factory;

		protected ParameterValidatorBase( Profile profile, Func<object, ParameterRelay> factory )
		{
			this.profile = profile;
			this.factory = factory;
		}

		public override bool CompileTimeValidate( Type type ) => Maps.Any( pair => pair.Value.Any() );

		IDictionary<string, MethodInfo[]> Maps { get; set; }

		public override void CompileTimeInitialize( Type type, AspectInfo aspectInfo ) => Maps = CreateMaps( type );

		IDictionary<string, MethodInfo[]> CreateMaps( Type type )
		{
			var methods = type.Adapt().GetMappedMethods( profile.Type );
			var result = new[] { profile.IsAllowed, profile.Execute }
				.ToDictionary( s => s, s => methods.Where( pair => pair.Item1.Name == s && pair.Item2.DeclaringType == type && !pair.Item2.IsAbstract && ( pair.Item2.IsFinal || pair.Item2.IsVirtual ) )
												  .Select( pair => pair.Item2 )
												  .ToArray() );
			// MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"{this} {name}: {type} ({map.Item2})", null, null, null ) );
			return result;
		}

		[OnMethodInvokeAdvice, MethodPointcut( nameof(FindIsAllowed) )]
		public void IsAllowed( MethodInterceptionArgs args )
		{
			var parameter = new RelayParameter<bool>( args.GetReturnValue<bool>, args.Arguments.Single() );
			args.ReturnValue = factory( args.Instance ).IsValid( parameter );
		}

		IEnumerable<MethodInfo> FindIsAllowed( Type type ) => Maps[ profile.IsAllowed ];
			
		[OnMethodInvokeAdvice, MethodPointcut( nameof(FindExecute) )]
		public void OnExecute( MethodInterceptionArgs args )
		{
			var parameter = new RelayParameter<object>( args.GetReturnValue, args.Arguments.Single() );
			args.ReturnValue = factory( args.Instance ).Execute( parameter );
		}

		IEnumerable<MethodInfo> FindExecute( Type type ) => Maps[ profile.Execute ];
	}

	public sealed class FactoryParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<object, ParameterRelay> Property = new AttachedProperty<ParameterRelay>( FactoryParameterStore.Instance );

		public FactoryParameterValidator() : 
			base( new Profile( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), Property.Get ) {}
	}

	public sealed class GenericFactoryParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<object, ParameterRelay> Property = new AttachedProperty<ParameterRelay>( GenericFactoryParameterStore.Instance );

		public GenericFactoryParameterValidator() : 
			base( new Profile( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), Property.Get ) {}
	}

	public sealed class CommandParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<object, ParameterRelay> Property = new AttachedProperty<ParameterRelay>( CommandParameterStore.Instance );

		public CommandParameterValidator() : 
			base( new Profile( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ), Property.Get ) {}
	}

	public sealed class GenericCommandParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<object, ParameterRelay> Property = new AttachedProperty<ParameterRelay>( GenericCommandParameterStore.Instance );

		public GenericCommandParameterValidator() : 
			base( new Profile( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ), Property.Get ) {}
	}

	class DelegatedParameterAware : IParameterAware
	{
		readonly IsValid valid;
		readonly Execute execute;

		/*public DelegatedParameterAware( IsValid valid, Action<object> execute ) : this( valid, new Execute( parameter =>
																											{
																												execute( parameter );
																												return null;
																											} ) ) {}*/

		public DelegatedParameterAware( IsValid valid, Execute execute )
		{
			this.valid = valid;
			this.execute = execute;
		}

		public bool IsValid( object parameter ) => valid( parameter );

		public object Execute( object parameter ) => execute( parameter );
	}

	/*public interface IAssignableParameterAware : IParameterAware
	{
		void Assign( IsValid condition );

		void Assign( Execute execute );
	}*/
}
