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

		public RelayParameter( MethodInterceptionArgs args, object parameter )
		{
			this.args = args;
			Parameter = parameter;
		}

		// public void Assign<T>( T result ) => args.ReturnValue = result;

		public T Proceed<T>() => args.GetReturnValue<T>();
		public object Parameter { get; }
	}

	public interface IParameterValidator
	{
		bool IsValid( object parameter );
	}

	public class FactoryAdapter<TParameter, TResult> : IGenericParameterValidator
	{
		readonly IFactory<TParameter, TResult> inner;
		public FactoryAdapter( IFactory<TParameter, TResult> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanCreate( (TParameter)parameter );

		public bool Handles( object parameter ) => parameter is TParameter;

		public object Execute( object parameter ) => inner.Create( (TParameter)parameter );
	}

	public class FactoryAdapter : IParameterValidator
	{
		readonly IFactoryWithParameter factory;

		public FactoryAdapter( IFactoryWithParameter factory )
		{
			this.factory = factory;
		}

		public bool IsValid( object parameter ) => factory.CanCreate( parameter );

		// public object Execute( object parameter ) => factory.Create( parameter );
	}

	public class CommandAdapter : IParameterValidator
	{
		readonly ICommand inner;
		public CommandAdapter( ICommand inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanExecute( parameter );
	}

	public class CommandAdapter<T> : IGenericParameterValidator
	{
		readonly ICommand<T> inner;
		public CommandAdapter( ICommand<T> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanExecute( (T)parameter );

		public bool Handles( object parameter ) => parameter is T;

		public object Execute( object parameter )
		{
			inner.Execute( (T)parameter );
			return null;
		}
	}

	public interface IParameterValidationController
	{
		bool IsValid( object parameter );

		void MarkValid( object parameter, bool valid );

		object Execute( RelayParameter parameter );
	}

	public class ParameterValidationController : IParameterValidationController
	{
		readonly protected static object Null = new object();

		readonly IParameterValidator validator;
		readonly IWritableStore<object> validated = new ThreadLocalStore<object>();
		
		public ParameterValidationController( IParameterValidator validator )
		{
			this.validator = validator;
		}

		public bool IsValid( object parameter ) => Equals( validated.Value, parameter ?? Null );

		public void MarkValid( object parameter, bool valid ) => validated.Assign( valid ? parameter ?? Null : null );

		protected virtual bool PerformValidation( object parameter ) => validator.IsValid( parameter );

		public virtual object Execute( RelayParameter parameter ) => IsValid( parameter.Parameter ) || PerformValidation( parameter.Parameter ) ? Proceed( parameter ) : null;

		protected object Proceed( RelayParameter parameter )
		{
			var result = parameter.Proceed<object>();
			validated.Assign( null );
			return result;
		}
	}

	public interface IGenericParameterValidator : IParameterValidator
	{
		bool Handles( object parameter );

		object Execute( object parameter );
	}

	public sealed class GenericParameterValidationController : ParameterValidationController// , IGenericParameterValidationController
	{
		readonly IGenericParameterValidator generic;

		readonly IWritableStore<object> active = new ThreadLocalStore<object>();

		public GenericParameterValidationController( IGenericParameterValidator generic, IParameterValidator validator ) : base( validator )
		{
			this.generic = generic;
		}

		protected override bool PerformValidation( object parameter ) => generic.Handles( parameter ) ? generic.IsValid( parameter ) : base.PerformValidation( parameter );

		public override object Execute( RelayParameter parameter )
		{
			var key = parameter.Parameter ?? Null;
			var handle = generic.Handles( parameter.Parameter ) && active.Value == null;
			if ( handle )
			{
				using ( active.Assignment( key ).Configured( false ) )
				{
					var result = generic.Execute( parameter.Parameter );
					return result;
				}
			}
			
			return base.Execute( parameter );
		}

		// public object ExecuteGeneric( RelayParameter parameter ) => base.Execute( parameter );
	}
	
}