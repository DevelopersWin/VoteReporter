using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
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

	/*public delegate bool IsValid( object parameter );

	public delegate object Execute( object parameter );*/

	public struct Profile
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

	abstract class ParameterAdapterStoreBase<T> : ProjectedStore<T, IParameterValidator>
	{
		/*protected override IParameterAware CreateValue( object instance ) => instance is T ? Project( (T)instance ) : null;

		protected abstract IParameterAware Project( T instance );*/
	}

	class WorkflowState : ThreadLocalAttachedProperty<ParameterState>
	{
		public static WorkflowState Property { get; } = new WorkflowState();

		WorkflowState() : base( () => new ParameterState() ) {}
	}

	abstract class GenericParameterAdapterStoreBase : ParameterAdapterStoreBase<object>
	{
		readonly Type genericType;
		readonly string methodName;
		readonly TypeAdapter adapter;

		protected GenericParameterAdapterStoreBase( Type genericType, string methodName = nameof(Create) )
		{
			this.genericType = genericType;
			this.methodName = methodName;
			adapter = GetType().Adapt();
		}

		protected override IParameterValidator Project( object instance )
		{
			var arguments = instance.GetType().Adapt().GetTypeArgumentsFor( genericType );
			var result = adapter.Invoke<IParameterValidator>( methodName, arguments, instance.ToItem() );
			return result;
		}
	}

	class GenericFactoryParameterAdapterStore : GenericParameterAdapterStoreBase
	{
		public static GenericFactoryParameterAdapterStore Instance { get; } = new GenericFactoryParameterAdapterStore();

		GenericFactoryParameterAdapterStore() : base( typeof(IFactory<,>), nameof(Create) ) {}

		static IParameterValidator Create<TParameter, TResult>( IFactory<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}
	class GenericCommandParameterAdapterStore : GenericParameterAdapterStoreBase
	{
		public static GenericCommandParameterAdapterStore Instance { get; } = new GenericCommandParameterAdapterStore();

		GenericCommandParameterAdapterStore() : base( typeof(ICommand<>), nameof(Create) ) {}

		static IParameterValidator Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
	}

	class CommandParameterAdapterStore : ParameterAdapterStoreBase<ICommand>
	{
		public static CommandParameterAdapterStore Instance { get; } = new CommandParameterAdapterStore();

		protected override IParameterValidator Project( ICommand instance ) => new CommandAdapter( instance );
	}

	class FactoryParameterAdapterStore : ParameterAdapterStoreBase<IFactoryWithParameter>
	{
		public static FactoryParameterAdapterStore Instance { get; } = new FactoryParameterAdapterStore();

		protected override IParameterValidator Project( IFactoryWithParameter instance ) => new FactoryAdapter( instance );
	}

	/*public interface IStateProvider
	{
		IParameterWorkflowState Get();
	}*/

	/*public struct StateProvider
	{
		readonly object instance;

		public StateProvider( object instance )
		{
			this.instance = instance;
		}

		// [Pure]
		public IParameterWorkflowState Get() => WorkflowState.Property.Get( instance );
	}*/

	/*public struct ParameterRelay
	{
		readonly IParameterAware adapter;
		readonly StateProvider state;

		public ParameterRelay( IParameterAware adapter, StateProvider state )
		{
			this.adapter = adapter;
			this.state = state;
		}

		public void IsValid( RelayParameter<bool> parameter )
		{
			var workflow = new ParameterInvocation( state.Get(), new ParameterWorkflowContext( adapter, parameter ) );
			workflow.IsValid( parameter.Parameter );
		}

		public void Execute( RelayParameter<object> parameter )
		{
			var workflow = new ParameterInvocation( state.Get(), new ParameterWorkflowContext( adapter, execute: parameter ) );
			workflow.Execute( parameter.Parameter );
		}
	}*/

	public struct RelayParameter
	{
		readonly MethodInterceptionArgs args;

		public RelayParameter( MethodInterceptionArgs args, object parameter )
		{
			this.args = args;
			Parameter = parameter;
		}

		public T Proceed<T>() => args.GetReturnValue<T>();
		public object Parameter { get; }
	}
	
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ParameterValidatorBase : TypeLevelAspect
	{
		readonly Profile profile;
		readonly Func<object, IParameterValidator> factory;

		protected ParameterValidatorBase( Profile profile, Func<object, IParameterValidator> factory )
		{
			this.profile = profile;
			this.factory = factory;
		}

		public override void CompileTimeInitialize( Type type, AspectInfo aspectInfo ) => Maps = CreateMaps( type );

		public override bool CompileTimeValidate( Type type ) => Maps.Any( pair => pair.Value.Any() );

		IDictionary<string, MethodInfo[]> Maps { get; set; }

		IEnumerable<MethodInfo> FindIsAllowed( Type type ) => Maps[ profile.IsAllowed ];
		IEnumerable<MethodInfo> FindExecute( Type type ) => Maps[ profile.Execute ];

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
			var parameter = new RelayParameter( args, args.Arguments.Single() );
			var state = WorkflowState.Property.Get( args.Instance );
			var workflow = new ValidationInvocation<RelayInvocation>( state.Active, state.Valid, new RelayInvocation( parameter ) );
			workflow.Invoke( parameter.Parameter );
		}

		[OnMethodInvokeAdvice, MethodPointcut( nameof(FindExecute) )]
		public void OnExecute( MethodInterceptionArgs args )
		{
			using ( new ThreadCacheContext() )
			{
				var adapter = factory( args.Instance );
				var state = WorkflowState.Property.Get( args.Instance );
				var validation = new ValidationInvocation<AdapterInvocation>( state.Active, state.Valid, new AdapterInvocation( adapter ) );
				var parameter = new RelayParameter( args, args.Arguments.Single() );
				var invocation = new ParameterInvocation( state, validation, parameter );
				invocation.Invoke( parameter.Parameter );
			}
		}
	}

	public sealed class FactoryParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<IParameterValidator> Property = new AttachedProperty<IParameterValidator>( FactoryParameterAdapterStore.Instance );

		public FactoryParameterValidator() : 
			base( new Profile( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), Property.Get ) {}
	}

	public sealed class GenericFactoryParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<IParameterValidator> Property = new AttachedProperty<IParameterValidator>( GenericFactoryParameterAdapterStore.Instance );

		public GenericFactoryParameterValidator() : 
			base( new Profile( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), Property.Get ) {}
	}

	public sealed class CommandParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<IParameterValidator> Property = new AttachedProperty<IParameterValidator>( CommandParameterAdapterStore.Instance );

		public CommandParameterValidator() : 
			base( new Profile( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ), Property.Get ) {}
	}

	public sealed class GenericCommandParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<IParameterValidator> Property = new AttachedProperty<IParameterValidator>( GenericCommandParameterAdapterStore.Instance );

		public GenericCommandParameterValidator() : 
			base( new Profile( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ), Property.Get ) {}
	}

	/*class DelegatedParameterAware : IParameterAware
	{
		readonly IsValid valid;
		readonly Execute execute;

		/*public DelegatedParameterAware( IsValid valid, Action<object> execute ) : this( valid, new Execute( parameter =>
																											{
																												execute( parameter );
																												return null;
																											} ) ) {}#1#

		public DelegatedParameterAware( IsValid valid, Execute execute )
		{
			this.valid = valid;
			this.execute = execute;
		}

		public bool IsValid( object parameter ) => valid( parameter );

		public object Execute( object parameter ) => execute( parameter );
	}*/

	/*public interface IAssignableParameterAware : IParameterAware
	{
		void Assign( IsValid condition );

		void Assign( Execute execute );
	}*/
}
