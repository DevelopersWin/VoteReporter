using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Collections.Immutable;
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
					Creator.Default.Set( args.ReturnValue, creator );
				}
			}
		}
	}

	public static class Properties
	{
		public static ICache<InstanceServiceProvider> Services = new ActivatedCache<InstanceServiceProvider>();
	}

	public struct AutoValidationProfile
	{
		public AutoValidationProfile( Func<object, IParameterValidator> factory, ImmutableArray<AutoValidationTypeDescriptor> descriptors )
		{
			Factory = factory;
			Descriptors = descriptors;
		}

		public Func<object, IParameterValidator> Factory { get; }
		public ImmutableArray<AutoValidationTypeDescriptor> Descriptors { get; }
	}

	public struct AutoValidationTypeDescriptor
	{
		public AutoValidationTypeDescriptor( Type type, string isValid, string execute )
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

	abstract class GenericParameterAdapterFactoryBase : FactoryBase<object, IParameterValidator>
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

		public override IParameterValidator Create( object parameter )
		{
			var arguments = parameter.GetType().Adapt().GetTypeArgumentsFor( genericType );
			var result = invoker.Invoke<IParameterValidator>( methodName, arguments, parameter.ToItem() );
			return result;
		}
	}
	
	class GenericFactoryAdapterFactory : GenericParameterAdapterFactoryBase
	{
		public static GenericFactoryAdapterFactory Instance { get; } = new GenericFactoryAdapterFactory();

		GenericFactoryAdapterFactory() : base( typeof(GenericFactoryAdapterFactory), typeof(IFactory<,>) ) {}

		static IParameterValidator Create<TParameter, TResult>( IFactory<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}
	class GenericCommandAdapterFactory : GenericParameterAdapterFactoryBase
	{
		public static GenericCommandAdapterFactory Instance { get; } = new GenericCommandAdapterFactory();

		GenericCommandAdapterFactory() : base( typeof(GenericCommandAdapterFactory), typeof(ICommand<>) ) {}

		static IParameterValidator Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
	}

	class CommandAdapterFactory : ParameterAdapterFactoryBase<ICommand>
	{
		public static CommandAdapterFactory Instance { get; } = new CommandAdapterFactory();
		CommandAdapterFactory() : base( command => new CommandAdapter( command ) ) {}
	}

	class FactoryAdapterFactory : ParameterAdapterFactoryBase<IFactoryWithParameter>
	{
		public static FactoryAdapterFactory Instance { get; } = new FactoryAdapterFactory();
		FactoryAdapterFactory() : base( parameter => new FactoryAdapter( parameter ) ) {}
	}

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

	public interface IParameterValidator
	{
		bool IsValid( object parameter );
	}

	public class FactoryAdapter<TParameter, TResult> : FactoryAdapter
	{
		readonly IFactory<TParameter, TResult> inner;
		public FactoryAdapter( IFactory<TParameter, TResult> inner ) : base( inner )
		{
			this.inner = inner;
		}

		public override bool IsValid( object parameter ) => parameter is TParameter ? inner.CanCreate( (TParameter)parameter ) : base.IsValid( parameter );
	}

	public class FactoryAdapter : IParameterValidator
	{
		readonly IFactoryWithParameter factory;

		public FactoryAdapter( IFactoryWithParameter factory )
		{
			this.factory = factory;
		}

		public virtual bool IsValid( object parameter ) => factory.CanCreate( parameter );
	}

	public class CommandAdapter : IParameterValidator
	{
		readonly ICommand inner;
		public CommandAdapter( ICommand inner )
		{
			this.inner = inner;
		}

		public virtual bool IsValid( object parameter ) => inner.CanExecute( parameter );
	}

	public class CommandAdapter<T> : CommandAdapter
	{
		readonly ICommand<T> inner;
		public CommandAdapter( ICommand<T> inner ) : base( inner )
		{
			this.inner = inner;
		}

		public override bool IsValid( object parameter ) => parameter is T ? inner.CanExecute( (T)parameter ) : base.IsValid( parameter );
	}

	public interface IParameterValidationController
	{
		void MarkValid( object parameter, bool valid );

		object Execute( RelayParameter parameter );
	}

	public class ParameterValidationController : IParameterValidationController
	{
		readonly static object Null = new object();

		readonly IWritableStore<object> validated = new ThreadLocalStore<object>();
		readonly IParameterValidator adapter;

		public ParameterValidationController( IParameterValidator adapter )
		{
			this.adapter = adapter;
		}

		public void MarkValid( object parameter, bool valid ) => validated.Assign( valid ? parameter ?? Null : null );

		public object Execute( RelayParameter parameter )
		{
			var proceed = Equals( validated.Value, parameter.Parameter ?? Null ) || adapter.IsValid( parameter.Parameter );
			var result = proceed ? parameter.Proceed<object>() : null;
			validated.Assign( null );
			return result;
		}
	}
}