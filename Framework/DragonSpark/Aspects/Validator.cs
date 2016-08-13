using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
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

	abstract class AdapterSourceBase<T> : ProjectedSource<T, IParameterValidationAdapter> where T : class
	{
		protected AdapterSourceBase( Func<T, IParameterValidationAdapter> create ) : base( create ) {}
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

		GenericFactoryProfileFactory() : base( typeof(IValidatedParameterizedSource<,>), typeof(GenericFactoryProfileFactory), nameof(Create) ) {}

		static IParameterValidationAdapter Create<TParameter, TResult>( IValidatedParameterizedSource<TParameter, TResult> instance ) => new FactoryAdapter<TParameter, TResult>( instance );
	}

	sealed class GenericCommandProfileFactory : GenericParameterProfileFactoryBase
	{
		public static GenericCommandProfileFactory Instance { get; } = new GenericCommandProfileFactory();

		GenericCommandProfileFactory() : base( typeof(ICommand<>), typeof(GenericCommandProfileFactory), nameof(Create) ) {}

		static IParameterValidationAdapter Create<T>( ICommand<T> instance ) => new CommandAdapter<T>( instance );
	}

	class CommandProfileSource : AdapterSourceBase<ICommand>
	{
		public static CommandProfileSource Instance { get; } = new CommandProfileSource();
		CommandProfileSource() : base( instance => new CommandAdapter( instance ) ) {}
	}

	class SourceAdapterSource : AdapterSourceBase<IValidatedParameterizedSource>
	{
		public static SourceAdapterSource Instance { get; } = new SourceAdapterSource();
		SourceAdapterSource() : base( instance => new FactoryAdapter( instance ) ) {}
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

	public interface IParameterValidationAdapter : ISpecification<MethodInfo> {}

	public abstract class ParameterValidationAdapterBase<T> : DecoratedSpecification<T>, IParameterValidationAdapter
	{
		readonly Func<MethodInfo, bool> method;

		protected ParameterValidationAdapterBase( ISpecification<T> inner, MethodInfo method ) : this( inner, MethodEqualitySpecification.For( method ) ) {}

		protected ParameterValidationAdapterBase( ISpecification<T> inner, Func<MethodInfo, bool> method ) : base( inner )
		{
			this.method = method;
		}

		public bool IsSatisfiedBy( MethodInfo parameter ) => method( parameter );

		protected override bool Coerce( object parameter ) => parameter is MethodInfo ? IsSatisfiedBy( (MethodInfo)parameter ) : base.Coerce( parameter );
	}

	public class FactoryAdapter : ParameterValidationAdapterBase<object>
	{
		readonly static MethodInfo Method = typeof(IParameterizedSource).GetTypeInfo().GetDeclaredMethod( nameof(IParameterizedSource.Get) );

		public FactoryAdapter( ISpecification inner ) : base( new DelegatedSpecification<object>( inner.IsSatisfiedBy ), Method ) {}
	}

	public class FactoryAdapter<TParameter, TResult> : ParameterValidationAdapterBase<TParameter>
	{
		readonly static MethodInfo Method = typeof(IParameterizedSource<TParameter, TResult>).GetTypeInfo().GetDeclaredMethod( nameof(IParameterizedSource<TParameter, TResult>.Get) );
		
		public FactoryAdapter( ISpecification<TParameter> inner ) : base( inner, Method ) {}
	}

	public class CommandAdapter : ParameterValidationAdapterBase<object>
	{
		readonly static MethodInfo Method = typeof(ICommand).GetTypeInfo().GetDeclaredMethod( nameof(ICommand.Execute) );

		public CommandAdapter( ICommand inner ) : base( new DelegatedSpecification<object>( inner.CanExecute ), Method ) {}
	}

	public class CommandAdapter<T> : ParameterValidationAdapterBase<T>
	{
		readonly static MethodInfo Method = typeof(ICommand<T>).GetTypeInfo().GetDeclaredMethod( nameof(ICommand<T>.Execute) );

		public CommandAdapter( ICommand<T> inner ) : base( inner, Method ) {}
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