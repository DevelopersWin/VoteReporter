using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Sources;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Aspects
{
	[PSerializable]
	[ProvideAspectRole( "Data" ), LinesOfCodeAvoided( 1 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Tracing )]
	public sealed class OriginAttribute : OnMethodBoundaryAspect
	{
		public override void OnSuccess( MethodExecutionArgs args )
		{
			if ( args.ReturnValue != null )
			{
				var creator = args.Instance as ISource;
				if ( creator != null )
				{
					Origin.Default.Set( args.ReturnValue, creator );
				}
			}
		}
	}

	/*public struct AutoValidationProfile
	{
		public AutoValidationProfile( Func<object, IParameterValidationAdapter> factory, ImmutableArray<AutoValidationTypeDescriptor> descriptors )
		{
			Factory = factory;
			Descriptors = descriptors;
		}

		public Func<object, IParameterValidationAdapter> Factory { get; }
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
	}*/

	abstract class AdapterFactoryBase<T> : ProjectedFactory<T, IParameterValidationAdapter> where T : class
	{
		protected AdapterFactoryBase( Func<T, IParameterValidationAdapter> create ) : base( create ) {}
	}

	abstract class GenericParameterProfileFactoryBase : GenericInvocationFactory<object, IParameterValidationAdapter>
	{
		protected GenericParameterProfileFactoryBase( Type genericTypeDefinition, Type owningType, string methodName ) : base( genericTypeDefinition, owningType, methodName ) {}
	}

	/*public struct ParameterInstanceProfile
	{
		public ParameterInstanceProfile( IParameterValidationAdapter adapter, InstanceMethod key )
		{
			Adapter = adapter;
			Key = key;
		}

		public IParameterValidationAdapter Adapter { get; }
		public InstanceMethod Key { get; }
	}*/
	
	sealed class GenericFactoryProfileFactory : GenericParameterProfileFactoryBase
	{
		public static GenericFactoryProfileFactory Instance { get; } = new GenericFactoryProfileFactory();

		GenericFactoryProfileFactory() : base( typeof(IFactory<,>), typeof(GenericFactoryProfileFactory), nameof(Create) ) {}

		static IParameterValidationAdapter Create<TParameter, TResult>( IFactory<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}

	sealed class GenericCommandProfileFactory : GenericParameterProfileFactoryBase
	{
		public static GenericCommandProfileFactory Instance { get; } = new GenericCommandProfileFactory();

		GenericCommandProfileFactory() : base( typeof(ICommand<>), typeof(GenericCommandProfileFactory), nameof(Create) ) {}

		static IParameterValidationAdapter Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
	}

	class CommandProfileFactory : AdapterFactoryBase<ICommand>
	{
		public static CommandProfileFactory Instance { get; } = new CommandProfileFactory();
		CommandProfileFactory() : base( instance => new CommandAdapter( instance ) ) {}
	}

	class FactoryProfileFactory : AdapterFactoryBase<IFactoryWithParameter>
	{
		public static FactoryProfileFactory Instance { get; } = new FactoryProfileFactory();
		FactoryProfileFactory() : base( instance => new FactoryAdapter( instance ) ) {}
	}

	/*public struct AutoValidationParameter
	{
		readonly MethodInterceptionArgs args;

		public AutoValidationParameter( MethodInterceptionArgs args, object parameter )
		{
			this.args = args;
			Parameter = parameter;
		}

		public T Proceed<T>() => args.GetReturnValue<T>();
		public object Parameter { get; }
	}*/

	public interface IParameterValidationAdapter : IMethodAware
	{
		bool IsValid( object parameter );
	}

	public class FactoryAdapter : IParameterValidationAdapter
	{
		readonly static MethodInfo Method = typeof(IFactoryWithParameter).GetTypeInfo().GetDeclaredMethod( nameof(IFactoryWithParameter.Create) );

		readonly IFactoryWithParameter inner;

		public FactoryAdapter( IFactoryWithParameter inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanCreate( parameter );

		MethodInfo IMethodAware.Method => Method;
	}

	public class FactoryAdapter<TParameter, TResult> : IParameterValidationAdapter
	{
		readonly static MethodInfo Method = typeof(IFactory<TParameter, TResult>).GetTypeInfo().GetDeclaredMethod( nameof(IFactory<TParameter, TResult>.Create) );
		
		readonly IFactory<TParameter, TResult> inner;

		public FactoryAdapter( IFactory<TParameter, TResult> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => parameter is TParameter ? inner.CanCreate( (TParameter)parameter ) : inner.CanCreate( parameter );

		MethodInfo IMethodAware.Method => Method;
	}

	public class CommandAdapter : IParameterValidationAdapter
	{
		readonly static MethodInfo Method = typeof(ICommand).GetTypeInfo().GetDeclaredMethod( nameof(ICommand.Execute) );

		readonly ICommand inner;
		public CommandAdapter( ICommand inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanExecute( parameter );

		MethodInfo IMethodAware.Method => Method;
	}

	public class CommandAdapter<T> : IParameterValidationAdapter
	{
		readonly static MethodInfo Method = typeof(ICommand<T>).GetTypeInfo().GetDeclaredMethod( nameof(ICommand<T>.Execute) );

		readonly ICommand<T> inner;
		public CommandAdapter( ICommand<T> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => parameter is T ? inner.CanExecute( (T)parameter ) : inner.CanExecute( parameter );

		MethodInfo IMethodAware.Method => Method;
	}

	/*public interface IAutoValidationController
	{
		void MarkValid( object parameter, bool valid );

		object Execute( AutoValidationParameter parameter );
	}

	public class AutoValidationController : IAutoValidationController
	{
	
		readonly IWritableStore<object> validated = new ThreadLocalStore<object>();
		readonly IParameterValidationAdapter adapter;

		public AutoValidationController( IParameterValidationAdapter adapter )
		{
			this.adapter = adapter;
		}

		public void MarkValid( object parameter, bool valid ) => validated.Assign( valid ? parameter ?? SpecialValues.Null : null );

		public object Execute( AutoValidationParameter parameter )
		{
			var proceed = Equals( validated.Value, parameter.Parameter ?? SpecialValues.Null ) || adapter.IsValid( parameter.Parameter );
			var result = proceed ? parameter.Proceed<object>() : null;
			validated.Assign( null );
			return result;
		}
	}*/
}