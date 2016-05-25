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

	abstract class AdapterFactoryBase<T> : ProjectedStore<T, IParameterAware>
	{
		protected override IParameterAware CreateValue( object instance ) => new RelayParameterAware( base.CreateValue( instance ), () => WorkflowState.Property.Get( instance ) );
	}

	class WorkflowState : ThreadLocalAttachedProperty<IParameterWorkflowState>
	{
		public static WorkflowState Property { get; } = new WorkflowState();

		WorkflowState() : base( () => new ParameterWorkflowState() ) {}
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

	public class RelayParameterAware : IParameterAware
	{
		readonly IParameterAware adapter;
		readonly Func<IParameterWorkflowState> state;

		public RelayParameterAware( IParameterAware adapter, Func<IParameterWorkflowState> state )
		{
			this.adapter = adapter;
			this.state = state;
		}

		public bool IsValid( object parameter ) => parameter.AsTo<RelayParameter<bool>, bool>( IsValid );

		bool IsValid( RelayParameter<bool> parameter )
		{
			var workflow = new ParameterWorkflow( state(), o => parameter.Factory(), adapter.Execute );
			var result = workflow.IsValid( parameter.Parameter );
			return result;
		}

		public object Execute( object parameter ) => parameter.AsTo<RelayParameter<object>, object>( Execute );

		object Execute( RelayParameter<object> parameter )
		{
			var workflow = new ParameterWorkflow( state(), adapter.IsValid, o => parameter.Factory() );
			var result = workflow.Execute( parameter.Parameter );
			return result;
		}
	}

	public class RelayParameter<T>
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
		readonly Func<object, IParameterAware> factory;

		// protected ParameterValidatorBase( Profile profile, AssignedAttachedPropertyStore<object, IParameterAware> store ) : this( profile, new AttachedProperty<IParameterAware>( store ).Get ) {}

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
