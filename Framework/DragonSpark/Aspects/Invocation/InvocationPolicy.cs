using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Reflection;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Invocation
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
	//[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[ProvideAspectRole( StandardRoles.Validation )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class ExtensionPointAttribute : MethodInterceptionAspect
	{
		public override void RuntimeInitialize( MethodBase method ) => Point = ExtensionPoints.Default.Get( (MethodInfo)method );

		IExtensionPoint Point { get; set; }

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var context = Point.Get( args.Instance );
			if ( context != null )
			{
				context.Assign( new AspectInvocation( args.Arguments, args.GetReturnValue ) );
				args.ReturnValue = context.Invoke( args.Arguments.GetArgument( 0 ) ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}

	public static class Defaults
	{
		public static Func<Type, IPolicy> PolicySource { get; } = Activator.Default.Get<IPolicy>;
	}

	public sealed class ExtensionPoints : Cache<MethodBase, IExtensionPoint>
	{
		public static ExtensionPoints Default { get; } = new ExtensionPoints();
		ExtensionPoints() : base( _ => new ExtensionPoint() ) {}
	}

	public interface IExtensionPoint : IParameterizedSource<IInvocationContext>
	{
		// CompiledInvocation Compile( object instance );
	}
	sealed class ExtensionPoint : Cache<IInvocationContext>, IExtensionPoint
	{
		// readonly IParameterizedSource<Pair> pairs;
		public ExtensionPoint() : base( o => new InvocationContext() ) {}

		public override IInvocationContext Get( object instance )
		{
			var context = base.Get( instance );
			var result = context.IsSatisfiedBy( instance ) ? context : null;
			return result;
		}
	}

	public abstract class CommandInvocationBase<T> : InvocationBase<T, object>, IInvocation<T>
	{
		public sealed override object Invoke( T parameter )
		{
			Execute( parameter );
			return null;
		}

		public abstract void Execute( T parameter );
	}

	public abstract class InvocationBase<TParameter, TResult> : IInvocation<TParameter, TResult>
	{
		public abstract TResult Invoke( TParameter parameter );

		object IInvocation.Invoke( object parameter ) => Invoke( (TParameter)parameter );
	}

	public interface IInvocation<in T> : IInvocation<T, object> {}

	public interface IInvocation<in TParameter, out TResult> : IInvocation
	{
		TResult Invoke( TParameter parameter );
	}

	public static class Extensions
	{
		public static IInvocation<TParameter, TResult> Get<TParameter, TResult>( this IInvocationContext @this ) => @this.Get().Wrap<TParameter, TResult>();
		public static IInvocation<TParameter, TResult> Wrap<TParameter, TResult>( this IInvocation @this ) => @this as IInvocation<TParameter, TResult> ?? Wrappers<TParameter, TResult>.Default.Get( @this );
		sealed class Wrappers<TParameter, TResult> : Cache<IInvocation, IInvocation<TParameter, TResult>>
		{
			public static Wrappers<TParameter, TResult> Default { get; } = new Wrappers<TParameter, TResult>();
			Wrappers() : base( result => new WrappedInvocation<TParameter, TResult>( result ) ) {}
		}
	}

	class WrappedInvocation<TParameter, TResult> : IInvocation<TParameter, TResult>
	{
		readonly IInvocation invocation;

		public WrappedInvocation( IInvocation invocation )
		{
			this.invocation = invocation;
		}

		TResult IInvocation<TParameter, TResult>.Invoke( TParameter parameter ) => (TResult)Invoke( parameter );

		public object Invoke( object parameter ) => invocation.Invoke( parameter );
	}

	class DelegatedInvocation<TParameter, TResult> : IInvocation
	{
		readonly Func<TParameter, TResult> source;
		public DelegatedInvocation( Func<TParameter, TResult> source )
		{
			this.source = source;
		}

		public object Invoke( object parameter ) => source( (TParameter)parameter );
	}

	public interface IInvocation
	{
		object Invoke( object parameter );
	}

	public interface IPolicy : ICommand<object> {}

	public interface ICommand<in T>
	{
		void Execute( T parameter );
	}

	public abstract class PolicyBase : IPolicy
	{
		public abstract void Execute( object parameter );
	}

	public struct AspectInvocation : IInvocation
	{
		readonly Func<object> proceed;

		public AspectInvocation( Arguments arguments, Func<object> proceed )
		{
			Arguments = arguments;
			this.proceed = proceed;
		}

		public Arguments Arguments { get; }

		public object Invoke( object parameter )
		{
			Arguments.SetArgument( 0, parameter );
			var result = proceed();
			return result;
		}
	}

	public interface IInvocationContext : IAssignableSource<IInvocation>, IAssignable<AspectInvocation>, IInvocation, IComposable<ISpecification<object>>, ISpecification<object> {}
	sealed class InvocationContext : SuppliedSource<IInvocation>, IInvocationContext
	{
		readonly IInitialInvocationLink initial;

		public InvocationContext() : this( new InitialInvocationLink() ) {}

		public InvocationContext( IInitialInvocationLink initial ) : base( initial )
		{
			this.initial = initial;
		}

		public void Assign( AspectInvocation item ) => initial.Assign( item );

		public object Invoke( object parameter ) => Get().Invoke( parameter );

		public void Add( ISpecification<object> instance ) => Specification = Specification != null ? Specification.And( instance ) : instance;

		ISpecification<object> Specification { get; set; }

		public bool IsSatisfiedBy( object parameter ) => Specification?.IsSatisfiedBy( parameter ) ?? true;
	}

	public interface IInitialInvocationLink : IInvocation, IAssignable<AspectInvocation> {}
	public class InitialInvocationLink : ThreadLocalStore<AspectInvocation>, IInitialInvocationLink
	{
		public object Invoke( object parameter ) => Get().Invoke( parameter );
	}
}
