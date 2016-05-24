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

	class WorkflowState : AttachedPropertyBase<IParameterWorkflowState>
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
	public abstract class ParameterValidatorBase : TypeLevelAspect
	{
		readonly Profile profile;
		readonly Func<object, IController> factory;

		protected ParameterValidatorBase( Profile profile, Func<object, IController> factory )
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
			var controller = factory( args.Instance );
			args.ReturnValue = controller.IsAllowed( o => args.GetReturnValue<bool>(), args.Arguments.Single() );
		}

		IEnumerable<MethodInfo> FindIsAllowed( Type type ) => Maps[ profile.IsAllowed ];
			
		[OnMethodInvokeAdvice, MethodPointcut( nameof(FindExecute) )]
		public void OnExecute( MethodInterceptionArgs args )
		{
			var controller = factory( args.Instance );
			args.ReturnValue = controller.Execute( o => args.GetReturnValue(), args.Arguments.Single() );
		}

		IEnumerable<MethodInfo> FindExecute( Type type ) => Maps[ profile.Execute ];
	}

	public sealed class FactoryParameterValidator : ParameterValidatorBase
	{
		public FactoryParameterValidator() : 
			base( new Profile( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), FactoryControllerFactory.Instance.Create ) {}
	}

	public sealed class GenericFactoryParameterValidator : ParameterValidatorBase
	{
		public GenericFactoryParameterValidator() : 
			base( new Profile( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ), GenericFactoryControllerFactory.Instance.Create ) {}
	}

	public sealed class CommandParameterValidator : ParameterValidatorBase
	{
		public CommandParameterValidator() : 
			base( new Profile( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ), CommandControllerFactory.Instance.Create ) {}
	}

	public sealed class GenericCommandParameterValidator : ParameterValidatorBase
	{
		public GenericCommandParameterValidator() : 
			base( new Profile( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) ), GenericCommandControllerFactory.Instance.Create ) {}
	}

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
}
