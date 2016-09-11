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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
			var compiled = Point.Compile( args.Instance );
			if ( compiled != null )
			{
				compiled.Assign( new AspectInvocation( args.Arguments, args.GetReturnValue ) );
				args.ReturnValue = compiled.Invoke( args.Arguments.GetArgument( 0 ) ) ?? args.ReturnValue;
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

	public interface IExtensionPoint : IParameterizedSource<IComposable<IInvocationLink>>
	{
		CompiledInvocation Compile( object instance );
	}
	sealed class ExtensionPoint : IExtensionPoint
	{
		readonly IParameterizedSource<Pair> pairs;
		readonly IParameterizedSource<IInvocationChain> chains;

		public ExtensionPoint() : this( new Cache<IInvocationChain>( o => new InvocationChain() ) ) {}
		public ExtensionPoint( IParameterizedSource<IInvocationChain> chains ) : this( chains, new Pairs( chains ) ) {}

		ExtensionPoint( IParameterizedSource<IInvocationChain> chains, IParameterizedSource<Pair> pairs )
		{
			this.chains = chains;
			this.pairs = pairs;
		}

		public IComposable<IInvocationLink> Get( object parameter ) => chains.Get( parameter );

		public CompiledInvocation Compile( object instance )
		{
			var pair = pairs.Get( instance );
			var result = pair.Specification?.IsSatisfiedBy( instance ) ?? true ? pair.Instance : null;
			return result;
		}

		sealed class Pairs : Cache<Pair>
		{
			public Pairs( IParameterizedSource<IInvocationChain> chains ) : base( new Factory( chains ).Get ) {}
			
			sealed class Factory : IParameterizedSource<Pair>
			{
				readonly IParameterizedSource<IInvocationChain> chains;

				public Factory( IParameterizedSource<IInvocationChain> chains )
				{
					this.chains = chains;
				}

				public Pair Get( object parameter )
				{
					var links = chains.Get( parameter ).ToArray();
					var specifications = links.OfType<ISpecification<object>>().ToArray();
					var length = specifications.Length;
					var specification = length == 1 ? specifications[0] : length > 1 ? new AllSpecification<object>( specifications ) : null;
					var result = new Pair( new CompiledInvocation( links ), specification );
					return result;
				}
			}
		}

		sealed class Pair
		{
			public Pair( CompiledInvocation instance, ISpecification<object> specification = null )
			{
				Specification = specification;
				Instance = instance;
			}

			public ISpecification<object> Specification { get; }
			public CompiledInvocation Instance { get; }
		}
	}

	sealed class DelegatedInvocation<T> : IInvocation where T : IInvocation
	{
		readonly Func<T> source;
		public DelegatedInvocation( Func<T> source )
		{
			this.source = source;
		}
		public object Invoke( object parameter ) => source().Invoke( parameter );
	}

	public sealed class CompiledInvocation : ThreadLocalStore<AspectInvocation>, IInvocation
	{
		readonly IInvocation invocation;

		public CompiledInvocation( IInvocationLink[] chain )
		{
			invocation = Compile( chain );
		}

		// public bool Enabled => chain.Enabled;

		IInvocation Compile( IInvocationLink[] links )
		{
			IInvocation result = new DelegatedInvocation<AspectInvocation>( Get );
			foreach ( var link in links )
			{
				result = link.Get( result );
			}
			return result;
		}

		public object Invoke( object parameter ) => invocation.Invoke( parameter );

		// public bool IsSatisfiedBy( object parameter ) => specification.IsSatisfiedBy( parameter );
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

	public interface IInvocation
	{
		object Invoke( object parameter );
	}

	public abstract class InvocationFactoryBase<T> : InvocationFactoryBase<T, object> {}
	public abstract class InvocationFactoryBase<TParameter, TResult> : InvocationFactoryBase
	{
		protected abstract IInvocation<TParameter, TResult> Create( IInvocation<TParameter, TResult> parameter );

		public sealed override IInvocation Get( IInvocation parameter ) => Create( parameter as IInvocation<TParameter, TResult> ?? new Wrapper( parameter ) );

		sealed class Wrapper : IInvocation<TParameter, TResult>
		{
			readonly IInvocation inner;

			public Wrapper( IInvocation inner )
			{
				this.inner = inner;
			}

			public TResult Invoke( TParameter parameter ) => (TResult)inner.Invoke( parameter );

			public object Invoke( object parameter ) => inner.Invoke( parameter );
		}
	}

	public abstract class InvocationFactoryBase : AlterationBase<IInvocation>, IInvocationLink {}

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

	public interface IInvocationLink : IAlteration<IInvocation> {}

	public interface IInvocationChain : ICollection<IInvocationLink>, IComposable<IInvocationLink> {}
	sealed class InvocationChain : Collection<IInvocationLink>, IInvocationChain {}
}
