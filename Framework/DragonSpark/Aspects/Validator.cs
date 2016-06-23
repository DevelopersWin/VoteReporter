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

	abstract class ParameterAdapterFactoryBase<T> : ProjectedFactory<T, IParameterAwareAdapter> where T : class
	{
		protected ParameterAdapterFactoryBase( Func<T, IParameterAwareAdapter> create ) : base( create ) {}
	}

	abstract class GenericParameterAdapterFactoryBase : FactoryBase<object, IParameterAwareAdapter>
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

		public override IParameterAwareAdapter Create( object parameter )
		{
			var arguments = parameter.GetType().Adapt().GetTypeArgumentsFor( genericType );
			var result = invoker.Invoke<IParameterAwareAdapter>( methodName, arguments, parameter.ToItem() );
			return result;
		}
	}
	
	class GenericFactoryParameterAdapterFactory : GenericParameterAdapterFactoryBase
	{
		public static GenericFactoryParameterAdapterFactory Instance { get; } = new GenericFactoryParameterAdapterFactory();

		GenericFactoryParameterAdapterFactory() : base( typeof(GenericFactoryParameterAdapterFactory), typeof(IFactory<,>) ) {}

		static IParameterAwareAdapter Create<TParameter, TResult>( IFactory<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}
	class GenericCommandParameterAdapterFactory : GenericParameterAdapterFactoryBase
	{
		public static GenericCommandParameterAdapterFactory Instance { get; } = new GenericCommandParameterAdapterFactory();

		GenericCommandParameterAdapterFactory() : base( typeof(GenericCommandParameterAdapterFactory), typeof(ICommand<>) ) {}

		static IParameterAwareAdapter Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
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

	public interface IParameterAwareAdapter
	{
		bool IsValid( object parameter );

		// object Execute( object parameter );
	}

	public class FactoryAdapter<TParameter, TResult> : FactoryAdapter
	{
		readonly IFactory<TParameter, TResult> inner;
		public FactoryAdapter( IFactory<TParameter, TResult> inner ) : base( inner )
		{
			this.inner = inner;
		}

		public override bool IsValid( object parameter ) => parameter is TParameter ? inner.CanCreate( (TParameter)parameter ) : base.IsValid( parameter );

		// public override object Execute( object parameter ) => parameter is TParameter ? inner.Create( (TParameter)parameter ) : base.Execute( parameter );
	}

	public class FactoryAdapter : IParameterAwareAdapter
	{
		readonly IFactoryWithParameter factory;

		public FactoryAdapter( IFactoryWithParameter factory )
		{
			this.factory = factory;
		}

		public virtual bool IsValid( object parameter ) => factory.CanCreate( parameter );

		// public virtual object Execute( object parameter ) => factory.Create( parameter );
	}

	public class CommandAdapter : IParameterAwareAdapter
	{
		readonly ICommand inner;
		public CommandAdapter( ICommand inner )
		{
			this.inner = inner;
		}

		public virtual bool IsValid( object parameter ) => inner.CanExecute( parameter );
		/*public virtual object Execute( object parameter )
		{
			inner.Execute( parameter );
			return null;
		}*/
	}

	public class CommandAdapter<T> : CommandAdapter
	{
		readonly ICommand<T> inner;
		public CommandAdapter( ICommand<T> inner ) : base( inner )
		{
			this.inner = inner;
		}

		public override bool IsValid( object parameter ) => parameter is T ? inner.CanExecute( (T)parameter ) : base.IsValid( parameter );

		// public override object Execute( object parameter ) => parameter is T ? Execute( (T)parameter ) : base.Execute( parameter );

		object Execute( T parameter )
		{
			inner.Execute( parameter );
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

		readonly IParameterAwareAdapter adapter;
		readonly IWritableStore<object> validated = new ThreadLocalStore<object>();
		
		public ParameterValidationController( IParameterAwareAdapter adapter )
		{
			this.adapter = adapter;
		}

		public bool IsValid( object parameter ) => Equals( validated.Value, parameter ?? Null );

		public void MarkValid( object parameter, bool valid ) => validated.Assign( valid ? parameter ?? Null : null );

		protected virtual bool PerformValidation( object parameter ) => adapter.IsValid( parameter );

		public virtual object Execute( RelayParameter parameter ) => IsValid( parameter.Parameter ) || PerformValidation( parameter.Parameter ) ? Proceed( parameter ) : null;

		protected object Proceed( RelayParameter parameter )
		{
			var result = parameter.Proceed<object>();
			validated.Assign( null );
			return result;
		}
	}

	/*public interface IGenericParameterValidator : IParameterValidator
	{
		bool Handles( object parameter );

		object Execute( object parameter );
	}*/

	/*public sealed class GenericParameterValidationController : ParameterValidationController
	{
		readonly IGenericParameterValidator generic;

		readonly IWritableStore<object> active = new ThreadLocalStore<object>();

		public GenericParameterValidationController( IGenericParameterValidator generic, IParameterAwareAdapter adapter ) : base( adapter )
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
	}*/
	
}