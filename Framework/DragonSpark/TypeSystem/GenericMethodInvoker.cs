using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.TypeSystem
{
	public class GenericMethodInvoker
	{
		readonly ConcurrentDictionary<string, IGenericMethodContext> candidates = new ConcurrentDictionary<string, IGenericMethodContext>();

		readonly MethodInfo[] methods;
		readonly Func<string, IGenericMethodContext> create;

		public GenericMethodInvoker( Type type ) : this( type.GetRuntimeMethods().Where( info => info.IsGenericMethod ).ToArray() ) {}

		GenericMethodInvoker( MethodInfo[] methods )
		{
			this.methods = methods;
			create = Get;
		}

		public IGenericMethodContext this[ string methodName ] => candidates.GetOrAdd( methodName, create );

		IGenericMethodContext Get( string name )
		{
			var immutableArray = methods.Introduce( name, tuple => tuple.Item1.Name == tuple.Item2, tuple => new GenericMethodFactory.Descriptor( tuple.Item1 ) ).ToImmutableArray();
			var result = new ContextCache( immutableArray );	
			return result;
		}
	}

	public interface IGenericMethodContext : IDictionary<Type[], GenericMethodInvocationContext>, IProjectedCache<Type[], GenericMethodInvocationContext>
	{
		GenericMethodInvocationContext Make( params Type[] types );
	}

	class ContextCache : ProjectedCache<Type[], GenericMethodInvocationContext>, IGenericMethodContext
	{
		public ContextCache( ImmutableArray<GenericMethodFactory.Descriptor> descriptors ) : this( new GenericMethodFactory( descriptors ).Create ) {}
		ContextCache( Func<Type[], GenericMethodInvocationContext> resultSelector ) : base( resultSelector, StructuralEqualityComparer<Type[]>.Instance ) {}
		public GenericMethodInvocationContext Make( params Type[] types ) => Get( types );
	}

	public class GenericMethodInvocationContext : ProjectedCache<Type[], DelegateContext>
	{
		public GenericMethodInvocationContext( ImmutableArray<GenericMethodCandidate> candidates ) : this( new Factory( candidates ).Create ) {}
		GenericMethodInvocationContext( Func<Type[], DelegateContext> resultSelector ) : base( resultSelector, StructuralEqualityComparer<Type[]>.Instance ) {}

		public T StaticInvoke<T>() => StaticInvoke<T>( Items<object>.Default );

		public T StaticInvoke<T>( params object[] arguments ) => InvokeCore<T>( null, arguments );

		public T Invoke<T>( object instance ) => Invoke<T>( instance, Items<object>.Default );

		public T Invoke<T>( object instance, params object[] arguments ) => InvokeCore<T>( instance, arguments );

		T InvokeCore<T>( [Optional]object instance, object[] arguments ) => GetContext( arguments ).Invoke<T>( instance, arguments );

		DelegateContext GetContext( object[] arguments ) => Get( ObjectTypeFactory.Instance.Create( arguments ) );

		public void StaticCall() => StaticCall( Items<object>.Default );

		public void StaticCall( params object[] arguments ) => CallCore( null, arguments );

		public void Call( object instance ) => Call( instance, Items<object>.Default );

		public void Call( object instance, params object[] arguments ) => CallCore( instance, arguments );

		void CallCore( object instance, object[] arguments ) => GetContext( arguments ).Call( instance, arguments );

		class Factory : FactoryBase<Type[], DelegateContext>
		{
			readonly GenericMethodCandidate[] candidates;

			public Factory( ImmutableArray<GenericMethodCandidate> candidates )
			{
				this.candidates = candidates.ToArray();
			}

			public override DelegateContext Create( Type[] parameter ) => candidates.Introduce( parameter, tuple => tuple.Item1.Specification( tuple.Item2 ), tuple => new DelegateContext( DelegateCache.Instance.Get( tuple.Item1.Method ) ) ).Single();
		}
	}

	public class DelegateContext
	{
		readonly Delegate @delegate;

		public DelegateContext( Delegate @delegate )
		{
			this.@delegate = @delegate;
		}

		public T Invoke<T>( params object[] arguments ) => Invoke<T>( null, arguments );

		public T Invoke<T>( [Optional] object instance, params object[] arguments )
		{
			var factory = (Func<object, object[], object>)@delegate;
			var result = (T)factory( instance, arguments );
			return result;
		}

		public void Call( params object[] arguments ) => Call( null, arguments );

		public void Call( [Optional] object instance, params object[] arguments )
		{
			var action = (Action<object, object[]>)@delegate;
			action( instance, arguments );
		}
	}

	class GenericMethodFactory : FactoryBase<Type[], GenericMethodInvocationContext>
	{
		readonly static Func<ValueTuple<Descriptor, Type[]>, GenericMethodCandidate> Selector = CreateSelector;

		readonly Descriptor[] descriptors;

		public GenericMethodFactory( ImmutableArray<Descriptor> descriptors )
		{
			this.descriptors = descriptors.ToArray();
		}

		static GenericMethodCandidate CreateSelector( ValueTuple<Descriptor, Type[]> item )
		{
			try
			{
				return new GenericMethodCandidate( item.Item1.Method.MakeGenericMethod( item.Item2 ) );
			}
			catch ( ArgumentException e )
			{
				DiagnosticProperties.Logger.Get( typeof(TypeAdapter) ).Verbose( e, "Could not create a generic method for {Method} with types {Types}", item.Item1.Method, item.Item2 );
				return default(GenericMethodCandidate);
			}
		}

		internal struct Descriptor
		{
			public Descriptor( MethodInfo method ) : this( method, GenericMethodEqualitySpecification.Default.Get( method ).ToDelegate() ) {}

			public Descriptor( MethodInfo method, Func<Type[], bool> specification )
			{
				Method = method;
				Specification = specification;
				// Arguments = arguments;
			}

			public MethodInfo Method { get; }
			public Func<Type[], bool> Specification { get; }
			// public ISpecification<Type[]> Arguments { get; }
		}

		public override GenericMethodInvocationContext Create( Type[] parameter )
		{
			var candidates = descriptors.Introduce( parameter, tuple => tuple.Item1.Specification( tuple.Item2 ), Selector ).WhereAssigned().ToImmutableArray();
			var result = new GenericMethodInvocationContext( candidates );
			return result;
		}
	}

	public struct GenericMethodCandidate
	{
		public GenericMethodCandidate( MethodInfo method ) : this( method, CompatibleArgumentsSpecification.Default.Get( method ).ToDelegate() ) {}

		public GenericMethodCandidate( MethodInfo method, Func<Type[], bool> specification )
		{
			Method = method;
			Specification = specification;
		}

		public MethodInfo Method { get; }
		public Func<Type[], bool> Specification { get; }
	}

	class DelegateCache : Cache<MethodInfo, Delegate>
	{
		public static DelegateCache Instance { get; } = new DelegateCache();
		DelegateCache() : base( Create ) {}

		static Delegate Create( MethodInfo parameter )
		{
			var parameters = parameter.GetParameterTypes();
			var instance = Expression.Parameter( typeof(object), "instance" );
			var arguments = Expression.Parameter( typeof(object[]), "args" );

			var array = new Expression[parameters.Length];
			for ( var i = 0; i < parameters.Length; i++ )
			{
				var index = Expression.ArrayIndex( arguments, Expression.Constant( i ) );
				array[i] = Expression.Convert( index, parameters[i] );
			}

			var body = parameter.IsStatic ? Expression.Call( parameter, array ) : Expression.Call( Expression.Convert( instance, parameter.DeclaringType ), parameter, array );

			var result = Expression.Lambda( body, instance, arguments ).Compile();
			return result;
		}
	}

	class GenericMethodEqualitySpecification : SpecificationWithContextBase<Type[], MethodBase>
	{
		public static ICache<MethodBase, ISpecification<Type[]>> Default { get; } = new Cache<MethodBase, ISpecification<Type[]>>( method => new GenericMethodEqualitySpecification( method ) );
		GenericMethodEqualitySpecification( MethodBase method ) : base( method ) {}

		public override bool IsSatisfiedBy( Type[] parameter ) => Context.GetGenericArguments().Length == parameter.Length;
	}

	class ObjectTypeFactory : FactoryBase<object[], Type[]>
	{
		public static ObjectTypeFactory Instance { get; } = new ObjectTypeFactory();

		public override Type[] Create( object[] parameter )
		{
			var result = new Type[parameter.Length];
			for ( var i = 0; i < parameter.Length; i++ )
			{
				result[i] = parameter[i]?.GetType();
			}
			return result;
		}
	}

	class CompatibleArgumentsSpecification : SpecificationWithContextBase<Type[], CompatibleArgumentsSpecification.Parameter[]>
	{
		readonly int required;
		public static ICache<MethodBase, ISpecification<Type[]>> Default { get; } = new Cache<MethodBase, ISpecification<Type[]>>( method => new CompatibleArgumentsSpecification( method ) );

		readonly static Func<ValueTuple<Parameter, Type[]>, int, bool> SelectCompatible = Compatible;

		CompatibleArgumentsSpecification( MethodBase method ) : this( method.GetParameters().Select( info => new Parameter( info.ParameterType.Adapt(), info.IsOptional ) ).ToArray() ) {}

		CompatibleArgumentsSpecification( Parameter[] parameters ) : this( parameters, parameters.Count( info => !info.Optional ) ) {}

		CompatibleArgumentsSpecification( Parameter[] parameters, int required ) : base( parameters )
		{
			this.required = required;
		}

		public struct Parameter
		{
			public Parameter( TypeAdapter parameterType, bool optional )
			{
				ParameterType = parameterType;
				Optional = optional;
			}

			public TypeAdapter ParameterType { get; }
			public bool Optional { get; }
		}

		public override bool IsSatisfiedBy( Type[] parameter )
		{
			var result = parameter.Length >= required && parameter.Length <= Context.Length
						 &&
						 Context.Introduce( parameter ).Select( SelectCompatible ).All();
			return result;
		}

		static bool Compatible( ValueTuple<Parameter, Type[]> context, int i )
		{
			var type = context.Item2.ElementAtOrDefault( i );
			var result = type != null ? context.Item1.ParameterType.IsAssignableFrom( type ) : i < context.Item2.Length || context.Item1.Optional;
			return result;
		}
	}
}