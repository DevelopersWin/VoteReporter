using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Linq;
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
				var creator = args.Instance as ICreator;
				if ( creator != null )
				{
					args.ReturnValue.Set( Creator.Default, creator );
				}
			}
		}
	}

	public static class Properties
	{
		public static ICache<InstanceServiceProvider> Services = new ActivatedCache<InstanceServiceProvider>();
	}

	public struct Profile
	{
		public Profile( Type type, string isValid, string execute )
		{
			Type = type;
			IsValid = isValid;
			Execute = execute;
		}

		public Type Type { get; }
		public string IsValid { get; }
		public string Execute { get; }
	}

	abstract class ParameterAdapterFactoryBase<T> : ProjectedFactory<T, IParameterValidator> where T : class
	{
		protected ParameterAdapterFactoryBase( Func<T, IParameterValidator> create ) : base( create ) {}
	}

	abstract class GenericParameterAdapterFactoryBase : FactoryBase<object, IGenericParameterValidator>
	{
		readonly Type genericType;
		readonly string methodName;
		readonly GenericMethodInvoker invoker;

		protected GenericParameterAdapterFactoryBase( Type parentType, Type genericType, string methodName = nameof(Create) )
		{
			this.genericType = genericType;
			this.methodName = methodName;
			invoker = parentType.Adapt().GenericMethods;
		}

		public override IGenericParameterValidator Create( object parameter )
		{
			var arguments = parameter.GetType().Adapt().GetTypeArgumentsFor( genericType );
			var result = invoker.Invoke<IGenericParameterValidator>( methodName, arguments, parameter.ToItem() );
			return result;
		}
	}
	
	class GenericFactoryParameterAdapterFactory : GenericParameterAdapterFactoryBase
	{
		public static GenericFactoryParameterAdapterFactory Instance { get; } = new GenericFactoryParameterAdapterFactory();

		GenericFactoryParameterAdapterFactory() : base( typeof(GenericFactoryParameterAdapterFactory), typeof(IFactory<,>) ) {}

		static IGenericParameterValidator Create<TParameter, TResult>( IFactory<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}
	class GenericCommandParameterAdapterFactory : GenericParameterAdapterFactoryBase
	{
		public static GenericCommandParameterAdapterFactory Instance { get; } = new GenericCommandParameterAdapterFactory();

		GenericCommandParameterAdapterFactory() : base( typeof(GenericCommandParameterAdapterFactory), typeof(ICommand<>) ) {}

		static IParameterValidator Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
	}

	class CommandParameterAdapterFactory : ParameterAdapterFactoryBase<ICommand>
	{
		public static CommandParameterAdapterFactory Instance { get; } = new CommandParameterAdapterFactory();
		CommandParameterAdapterFactory() : base( command => new CommandAdapter( command ) ) {}
	}

	class FactoryParameterAdapterFactory : ParameterAdapterFactoryBase<IFactoryWithParameter>
	{
		public static FactoryParameterAdapterFactory Instance { get; } = new FactoryParameterAdapterFactory();
		FactoryParameterAdapterFactory() : base( parameter => new FactoryAdapter( parameter ) ) {}
	}

	public struct RelayParameter
	{
		readonly MethodInterceptionArgs args;

		public RelayParameter( MethodInterceptionArgs args ) : this( args, args.Arguments.Single() ) {}

		public RelayParameter( MethodInterceptionArgs args, object parameter )
		{
			this.args = args;
			Parameter = parameter;
		}

		// public void Assign<T>( T result ) => args.ReturnValue = result;

		public T Proceed<T>() => args.GetReturnValue<T>();
		public object Parameter { get; }
	}
	
}