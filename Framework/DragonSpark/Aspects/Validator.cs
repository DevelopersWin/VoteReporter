using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Collections.Immutable;
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
				var creator = args.Instance as ICreator;
				if ( creator != null )
				{
					Creator.Default.Set( args.ReturnValue, creator );
				}
			}
		}
	}

	public struct AutoValidationProfile
	{
		public AutoValidationProfile( Func<object, ParameterInstanceProfile> factory, ImmutableArray<AutoValidationTypeDescriptor> descriptors )
		{
			Factory = factory;
			Descriptors = descriptors;
		}

		public Func<object, ParameterInstanceProfile> Factory { get; }
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

	abstract class ParameterProfileFactoryBase<T> : ProjectedFactory<T, ParameterInstanceProfile> where T : class
	{
		protected ParameterProfileFactoryBase( Func<T, ParameterInstanceProfile> create ) : base( create ) {}
	}

	abstract class GenericParameterProfileFactoryBase : GenericInvocationFactory<object, ParameterInstanceProfile>
	{
		protected GenericParameterProfileFactoryBase( Type genericTypeDefinition, Type owningType, string methodName ) : base( genericTypeDefinition, owningType, methodName ) {}
	}

	public struct ParameterInstanceProfile
	{
		public ParameterInstanceProfile( IParameterValidationAdapter adapter, InstanceMethod key )
		{
			Adapter = adapter;
			Key = key;
		}

		public IParameterValidationAdapter Adapter { get; }
		public InstanceMethod Key { get; }
	}
	
	sealed class GenericFactoryProfileFactory : GenericParameterProfileFactoryBase
	{
		public static GenericFactoryProfileFactory Instance { get; } = new GenericFactoryProfileFactory();

		GenericFactoryProfileFactory() : base( typeof(IFactory<,>), typeof(GenericFactoryProfileFactory), nameof(Create) ) {}

		static ParameterInstanceProfile Create<TParameter, TResult>( IFactory<TParameter, TResult> instance ) => new ParameterInstanceProfile( new FactoryAdapter<TParameter, TResult>( instance ), new InstanceMethod( instance, Method<TParameter, TResult>.Default ) );

		public static class Method<TParameter, TResult>
		{
			public static MethodBase Default { get; } = typeof(IFactory<TParameter, TResult>).GetTypeInfo().GetDeclaredMethod( nameof(IFactory<TParameter, TResult>.Create) );
		}
	}
	sealed class GenericCommandProfileFactory : GenericParameterProfileFactoryBase
	{
		public static GenericCommandProfileFactory Instance { get; } = new GenericCommandProfileFactory();

		GenericCommandProfileFactory() : base( typeof(ICommand<>), typeof(GenericCommandProfileFactory), nameof(Create) ) {}

		static ParameterInstanceProfile Create<T>( ICommand<T> instance ) => new ParameterInstanceProfile( new CommandAdapter<T>( instance ), new InstanceMethod( instance, Cache<T>.Method ) );

		static class Cache<T>
		{
			public static MethodBase Method { get; } = typeof(ICommand<T>).GetTypeInfo().GetDeclaredMethod( nameof(ICommand<T>.Execute) );
		}
	}

	class CommandProfileFactory : ParameterProfileFactoryBase<ICommand>
	{
		readonly static MethodBase Method = typeof(ICommand).GetTypeInfo().GetDeclaredMethod( nameof(ICommand.Execute) );

		public static CommandProfileFactory Instance { get; } = new CommandProfileFactory();
		CommandProfileFactory() : base( instance => new ParameterInstanceProfile( new CommandAdapter( instance ), new InstanceMethod( instance, Method ) ) ) {}
	}

	class FactoryProfileFactory : ParameterProfileFactoryBase<IFactoryWithParameter>
	{
		public static MethodBase Method { get; } = typeof(IFactoryWithParameter).GetTypeInfo().GetDeclaredMethod( nameof(IFactoryWithParameter.Create) );

		public static FactoryProfileFactory Instance { get; } = new FactoryProfileFactory();
		FactoryProfileFactory() : base( instance => new ParameterInstanceProfile( new FactoryAdapter( instance ), new InstanceMethod( instance, Method ) ) ) {}
	}

	public struct AutoValidationParameter
	{
		readonly MethodInterceptionArgs args;

		public AutoValidationParameter( MethodInterceptionArgs args, object parameter )
		{
			this.args = args;
			Parameter = parameter;
		}

		public T Proceed<T>() => args.GetReturnValue<T>();
		public object Parameter { get; }
	}

	public interface IParameterValidationAdapter
	{
		bool IsValid( object parameter );
	}

	public class FactoryAdapter : IParameterValidationAdapter
	{
		readonly IFactoryWithParameter inner;

		public FactoryAdapter( IFactoryWithParameter inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanCreate( parameter );
	}

	public class FactoryAdapter<TParameter, TResult> : IParameterValidationAdapter
	{
		readonly IFactory<TParameter, TResult> inner;

		public FactoryAdapter( IFactory<TParameter, TResult> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => parameter is TParameter ? inner.CanCreate( (TParameter)parameter ) : inner.CanCreate( parameter );
	}

	public class CommandAdapter : IParameterValidationAdapter
	{
		readonly ICommand inner;
		public CommandAdapter( ICommand inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => inner.CanExecute( parameter );
	}

	public class CommandAdapter<T> : IParameterValidationAdapter
	{
		readonly ICommand<T> inner;
		public CommandAdapter( ICommand<T> inner )
		{
			this.inner = inner;
		}

		public bool IsValid( object parameter ) => parameter is T ? inner.CanExecute( (T)parameter ) : inner.CanExecute( parameter );
	}

	public interface IAutoValidationController
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
	}
}