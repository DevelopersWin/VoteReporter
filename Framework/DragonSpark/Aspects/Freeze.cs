using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect, IInstanceScopedAspect
	{
		readonly ArgumentCache cache = new ArgumentCache();

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			object result;
			var key = args.Arguments.ToArray();
			args.ReturnValue = cache.TryGetValue( key, out result ) ? result : cache.GetOrAdd( key, args.GetReturnValue() );
		}

		public object CreateInstance( AdviceArgs adviceArgs ) => new Freeze();

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}

		class ArgumentCache : ArgumentCache<MethodInterceptionArgs>
		{
			public ArgumentCache() : base( args => args.Arguments.ToArray(), args => args.GetReturnValue() ) {}
		}
	}

	class ArgumentCache : ArgumentCache<object[]>
	{
		public ArgumentCache( Func<object[], object> resultSelector ) : base( Delegates<object[]>.Self, resultSelector ) {}
	}

	class ArgumentCache<T> : ArgumentCache<T, object>
	{
		public ArgumentCache( Func<T, object[]> keySelector, Func<T, object> resultSelector ) : base( keySelector, resultSelector ) {}
	}

	class ArgumentCache<TContext, TResult> : ConcurrentDictionary<object[], TResult>
	{
		readonly Func<TContext, object[]> keySelector;
		readonly Func<TContext, TResult> resultSelector;
		public ArgumentCache( Func<TContext, object[]> keySelector, Func<TContext, TResult> resultSelector ) : base( StructuralEqualityComparer<object[]>.Instance )
		{
			this.keySelector = keySelector;
			this.resultSelector = resultSelector;
		}

		public TResult Get( TContext context )
		{
			TResult result;
			var key = keySelector( context );
			return TryGetValue( key, out result ) ? result : GetOrAdd( key, resultSelector( context ) );
		}
	}

	public class MethodInvocationSpecificationRepository : EntryRepositoryBase<ISpecification<MethodInvocationParameter>> {}

	public struct MethodInvocationParameter
	{
		readonly MethodInterceptionArgs args;
		// public static MethodInvocationParameter From( MethodInterceptionArgs args ) => new MethodInvocationParameter(  );

		public MethodInvocationParameter( MethodInterceptionArgs args ) : this( args.Method, args.Instance, args.Arguments.ToArray(), args ) {}

		public MethodInvocationParameter( MethodBase method, object instance, object[] arguments, MethodInterceptionArgs args )
		{
			Instance = instance;
			Method = method;
			Arguments = arguments;
			this.args = args;
		}

		public object Instance { get; }
		public MethodBase Method { get; }
		public object[] Arguments { get; }
		public T Proceed<T>() => args.GetReturnValue<T>();
	}
}