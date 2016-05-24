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

	/*public interface IController
	{
		bool IsAllowed( Func<object, bool> assign, object parameter );

		object Execute( Func<object, object> assign, object parameter );
	}*/

	public delegate bool IsValid( object parameter );

	public delegate object Execute( object parameter );

	/*class Controller : IController
	{
		readonly IParameterAware workflow;
		readonly IParameterAware adapter;

		public Controller( IParameterAware adapter )
		{
			this.adapter = adapter;
		}

		public bool IsAllowed( IsValid assign, object parameter )
		{
			
			/*using ( new IsAllowedAssignment( adapter, assign ).Configured( false ) )
			{
				return workflow.IsValid( parameter );
			}#1#
		}

		public object Execute( Func<object, object> assign, object parameter )
		{
			using ( new ExecuteAssignment( adapter, assign ).Configured( false ) )
			{
				return workflow.Execute( parameter );
			}
		}

		/*class IsAllowedAssignment : Assignment<IsValid>
		{
			public IsAllowedAssignment( IAssignableParameterAware adapter, Func<object, bool> first ) : base( adapter.Assign, new Value<Func<object, bool>>( first ) ) {}
		}

		class ExecuteAssignment : Assignment<Execute>
		{
			public ExecuteAssignment( IAssignableParameterAware adapter, Func<object, object> first ) : base( adapter.Assign, new Value<Func<object, object>>( first ) ) {}
		}#1#
	}*/

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

	/*public interface IControllerFactory
	{
		IController Create( object instance );
	}*/

	abstract class AdapterFactoryBase<T> : ProjectedStore<T, IParameterAware>
	{
		/*public IController Create( object instance )
		{
			var state = instance.Get( WorkflowState.Property );
			var assignable = new AssignableParameterAware( aware );
			var result = new Controller( new ParameterWorkflow( state, assignable ), assignable );
			return result;
		}*/

		
	}

	class WorkflowState : AttachedProperty<ParameterWorkflowState>
	{
		public static WorkflowState Property { get; } = new WorkflowState();

		WorkflowState() : base( ActivatedAttachedPropertyStore<object, ParameterWorkflowState>.Instance ) {}
	}

	abstract class GenericAdapterFactoryBase : AdapterFactoryBase<object>
	{
		readonly Type genericType;
		readonly string methodName;
		readonly TypeAdapter adapter;

		protected GenericAdapterFactoryBase( Type genericType, string methodName = nameof(Create) )
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

	class GenericFactoryAdapterFactory : GenericAdapterFactoryBase
	{
		public new static GenericFactoryAdapterFactory Instance { get; } = new GenericFactoryAdapterFactory();

		GenericFactoryAdapterFactory() : base( typeof(IFactory<,>), nameof(Create) ) {}

		static IParameterAware Create<TParameter, TResult>( IFactory<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}
	class GenericCommandAdapterFactory : GenericAdapterFactoryBase
	{
		public new static GenericCommandAdapterFactory Instance { get; } = new GenericCommandAdapterFactory();

		GenericCommandAdapterFactory() : base( typeof(ICommand<>), nameof(Create) ) {}

		static IParameterAware Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
	}

	/*class FactoryAdapterFactory<TParameter, TResult> : AdapterFactoryBase<IFactory<TParameter, TResult>>
	{
		public new static FactoryAdapterFactory<TParameter, TResult> Instance { get; } = new FactoryAdapterFactory<TParameter, TResult>();

		protected override IParameterAware Project( IFactory<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}

	class GenericCommandAdapterFactory<T> : AdapterFactoryBase<ICommand<T>>
	{
		public new static GenericCommandAdapterFactory<T> Instance { get; } = new GenericCommandAdapterFactory<T>();

		protected override IParameterAware Project( ICommand<T> instance ) => new CommandAdapter<T>( instance );
	}*/

	class CommandAdapterFactory : AdapterFactoryBase<ICommand>
	{
		public new static CommandAdapterFactory Instance { get; } = new CommandAdapterFactory();

		protected override IParameterAware Project( ICommand instance ) => new CommandAdapter( instance );
	}

	class FactoryAdapterFactory : AdapterFactoryBase<IFactoryWithParameter>
	{
		public new static FactoryAdapterFactory Instance { get; } = new FactoryAdapterFactory();

		protected override IParameterAware Project( IFactoryWithParameter instance ) => new DelegatedParameterAware( instance.CanCreate, instance.Create );
	}

	
	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ParameterValidatorBase : TypeLevelAspect
	{
		readonly Profile profile;
		readonly Func<object, IParameterAware> factory;

		// protected ParameterValidatorBase( Profile profile, AttachedPropertyStore<object, IParameterAware> store ) : this( profile, new AttachedProperty<IParameterAware>( store ).Get ) {}

		protected ParameterValidatorBase( Profile profile, Func<object, IParameterAware> factory )
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
			var adapter = factory( args.Instance );
			var state = WorkflowState.Property.Get( args.Instance );
			var workflow = new ParameterWorkflow( state, new DelegatedParameterAware( o => args.GetReturnValue<bool>(), adapter.Execute ) );
			args.ReturnValue = workflow.IsValid( args.Arguments.Single() );
		}

		IEnumerable<MethodInfo> FindIsAllowed( Type type ) => Maps[ profile.IsAllowed ];
			
		[OnMethodInvokeAdvice, MethodPointcut( nameof(FindExecute) )]
		public void OnExecute( MethodInterceptionArgs args )
		{
			var adapter = factory( args.Instance );
			var state = WorkflowState.Property.Get( args.Instance );
			var workflow = new ParameterWorkflow( state, new DelegatedParameterAware( adapter.IsValid, o => args.GetReturnValue() ) );
			args.ReturnValue = workflow.Execute( args.Arguments.Single() );
		}

		IEnumerable<MethodInfo> FindExecute( Type type ) => Maps[ profile.Execute ];
	}

	public sealed class FactoryParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<object, IParameterAware> Property = new AttachedProperty<IParameterAware>( FactoryAdapterFactory.Instance );

		public FactoryParameterValidator() : 
			base( new Profile( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), Property.Get ) {}
	}

	public sealed class GenericFactoryParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<object, IParameterAware> Property = new AttachedProperty<IParameterAware>( GenericFactoryAdapterFactory.Instance );

		public GenericFactoryParameterValidator() : 
			base( new Profile( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), Property.Get ) {}
	}

	public sealed class CommandParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<object, IParameterAware> Property = new AttachedProperty<IParameterAware>( CommandAdapterFactory.Instance );

		public CommandParameterValidator() : 
			base( new Profile( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ), Property.Get ) {}
	}

	public sealed class GenericCommandParameterValidator : ParameterValidatorBase
	{
		readonly static IAttachedProperty<object, IParameterAware> Property = new AttachedProperty<IParameterAware>( GenericCommandAdapterFactory.Instance );

		public GenericCommandParameterValidator() : 
			base( new Profile( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ), Property.Get ) {}
	}

	class DelegatedParameterAware : IParameterAware
	{
		readonly IsValid valid;
		readonly Execute execute;

		public DelegatedParameterAware( IsValid valid, Action<object> execute ) : this( valid, new Execute( parameter =>
																											{
																												execute( parameter );
																												return null;
																											} ) ) {}

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
