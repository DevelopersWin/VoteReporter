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
					args.ReturnValue.Set( Creator.Property, creator );
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

	abstract class ParameterAdapterCacheBase<T> : Cache<IParameterValidator> where T : class
	{
		protected ParameterAdapterCacheBase( Func<T, IParameterValidator> create ) : base( new ProjectedFactory<T, IParameterValidator>( create ).ToDelegate()  ) {}
	}

	abstract class GenericParameterAdapterStoreBase : Cache<IGenericParameterValidator>
	{
		protected GenericParameterAdapterStoreBase( Factory factory ) : base( factory.ToDelegate() ) {}

		protected class Factory : FactoryBase<object, IGenericParameterValidator>
		{
			readonly Type genericType;
			readonly string methodName;
			readonly TypeAdapter adapter;

			public Factory( Type parentType, Type genericType, string methodName = nameof(Create) )
			{
				this.genericType = genericType;
				this.methodName = methodName;
				adapter = parentType.Adapt();
			}

			public override IGenericParameterValidator Create( object parameter )
			{
				var arguments = parameter.GetType().Adapt().GetTypeArgumentsFor( genericType );
				var result = adapter.Invoke<IGenericParameterValidator>( methodName, arguments, parameter.ToItem() );
				return result;
			}
		}
	}

	class GenericFactoryParameterAdapterStore : GenericParameterAdapterStoreBase
	{
		public static GenericFactoryParameterAdapterStore Instance { get; } = new GenericFactoryParameterAdapterStore();

		GenericFactoryParameterAdapterStore() : base( new Factory( typeof(GenericFactoryParameterAdapterStore), typeof(IFactory<,>) ) ) {}

		static IGenericParameterValidator Create<TParameter, TResult>( IFactory<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}
	class GenericCommandParameterAdapterStore : GenericParameterAdapterStoreBase
	{
		public static GenericCommandParameterAdapterStore Instance { get; } = new GenericCommandParameterAdapterStore();

		GenericCommandParameterAdapterStore() : base( new Factory( typeof(GenericCommandParameterAdapterStore), typeof(ICommand<>) ) ) {}

		static IParameterValidator Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
	}

	class CommandParameterAdapterCache : ParameterAdapterCacheBase<ICommand>
	{
		public static CommandParameterAdapterCache Instance { get; } = new CommandParameterAdapterCache();
		CommandParameterAdapterCache() : base( command => new CommandAdapter( command ) ) {}
	}

	class FactoryParameterAdapterCache : ParameterAdapterCacheBase<IFactoryWithParameter>
	{
		public static FactoryParameterAdapterCache Instance { get; } = new FactoryParameterAdapterCache();
		FactoryParameterAdapterCache() : base( parameter => new FactoryAdapter( parameter ) ) {}
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