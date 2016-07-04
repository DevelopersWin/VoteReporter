using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Collections.Immutable;
using System.Windows.Input;
using Delegates = DragonSpark.Runtime.Delegates;

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
	}

	abstract class ParameterAdapterFactoryBase<T> : ProjectedFactory<T, IParameterValidationAdapter> where T : class
	{
		protected ParameterAdapterFactoryBase( Func<T, IParameterValidationAdapter> create ) : base( create ) {}
	}

	abstract class GenericParameterAdapterFactoryBase : FactoryBase<object, IParameterValidationAdapter>
	{
		readonly Type genericType;
		readonly IGenericMethodContext context;
		
		protected GenericParameterAdapterFactoryBase( Type genericType, Type parentType, string methodName = nameof(Create) ) : this( parentType.Adapt().GenericMethods[ methodName ], genericType ) {}

		GenericParameterAdapterFactoryBase( IGenericMethodContext context, Type genericType )
		{
			this.genericType = genericType;
			this.context = context;
		}

		public override IParameterValidationAdapter Create( object parameter )
		{
			var arguments = parameter.GetType().Adapt().GetTypeArgumentsFor( genericType );
			var result = context.Make( arguments ).StaticInvoke<IParameterValidationAdapter>( parameter );
			return result;
		}
	}
	
	class GenericFactoryAdapterFactory : GenericParameterAdapterFactoryBase
	{
		public static GenericFactoryAdapterFactory Instance { get; } = new GenericFactoryAdapterFactory();

		GenericFactoryAdapterFactory() : base( typeof(IFactory<,>), typeof(GenericFactoryAdapterFactory) ) {}

		static IParameterValidationAdapter Create<TParameter, TResult>( IFactory<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}
	class GenericCommandAdapterFactory : GenericParameterAdapterFactoryBase
	{
		public static GenericCommandAdapterFactory Instance { get; } = new GenericCommandAdapterFactory();

		GenericCommandAdapterFactory() : base( typeof(ICommand<>), typeof(GenericCommandAdapterFactory) ) {}

		static IParameterValidationAdapter Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
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

		object Execute( object parameter );

		Delegate GetFactory();
	}

	public class FactoryAdapter : IParameterValidationAdapter
	{
		readonly IFactoryWithParameter factory;

		public FactoryAdapter( IFactoryWithParameter factory )
		{
			this.factory = factory;
		}

		public virtual bool IsValid( object parameter ) => factory.CanCreate( parameter );

		public virtual object Execute( object parameter ) => factory.Create( parameter );
		public virtual Delegate GetFactory() => Delegates.Default.Lookup( factory.ToDelegate() );
	}

	public class FactoryAdapter<TParameter, TResult> : FactoryAdapter
	{
		readonly IFactory<TParameter, TResult> inner;

		public FactoryAdapter( IFactory<TParameter, TResult> inner ) : base( inner )
		{
			this.inner = inner;
		}

		public override bool IsValid( object parameter ) => parameter is TParameter ? inner.CanCreate( (TParameter)parameter ) : base.IsValid( parameter );

		public override object Execute( object parameter ) => inner.Create( (TParameter)parameter );

		public override Delegate GetFactory() => Delegates.Default.Lookup( inner.ToDelegate() );
	}

	public class CommandAdapter : IParameterValidationAdapter
	{
		readonly ICommand inner;
		public CommandAdapter( ICommand inner )
		{
			this.inner = inner;
		}

		public virtual bool IsValid( object parameter ) => inner.CanExecute( parameter );

		public virtual object Execute( object parameter )
		{
			inner.Execute( parameter );
			return null;
		}

		public virtual Delegate GetFactory() => Delegates.Default.Lookup( inner.ToDelegate() );
	}

	public class CommandAdapter<T> : CommandAdapter
	{
		readonly ICommand<T> inner;
		public CommandAdapter( ICommand<T> inner ) : base( inner )
		{
			this.inner = inner;
		}

		public override bool IsValid( object parameter ) => parameter is T ? inner.CanExecute( (T)parameter ) : base.IsValid( parameter );

		public override object Execute( object parameter )
		{
			inner.Execute( (T)parameter );
			return null;
		}

		public override Delegate GetFactory() => Delegates.Default.Lookup( inner.ToDelegate() );
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