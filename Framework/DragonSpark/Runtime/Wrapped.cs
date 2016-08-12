using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using Constructor = DragonSpark.Activation.Constructor;

namespace DragonSpark.Runtime
{
	public interface IDelegateInvoker
	{
		object Invoke( object[] arguments );
	}

	public static class DelegateInvokerExtensions
	{
		public static IDelegateInvoker Cached( this IDelegateInvoker @this ) => CachedInvoker.Default.Get( @this );
		class CachedInvoker : Cache<IDelegateInvoker, IDelegateInvoker>
		{
			public static ICache<IDelegateInvoker, IDelegateInvoker> Default { get; } = new CachedInvoker();
			CachedInvoker() : base( invoker => new CachedDelegateInvoker( invoker ) ) {}
		}

		public static Func<object[], object> ToDelegate( this IDelegateInvoker @this ) => DelegateCache.Default.Get( @this );

		class DelegateCache : Cache<IDelegateInvoker, Func<object[], object>>
		{
			public static DelegateCache Default { get; } = new DelegateCache();

			DelegateCache() : base( invoker => invoker.Invoke ) {}
		}
	}

	/*public class TypeArrayEqualityComparer : IEqualityComparer<Type[]>
	{
		public static TypeArrayEqualityComparer Instance { get; } = new TypeArrayEqualityComparer();

		public bool Equals( Type[] x, Type[] y )
		{
			if (x.Length != y.Length)
				return false;

			for (int i = 0; i < x.Length; i++)
				if (x[i] != y[i])
					return false;

			return true;

		}

		public int GetHashCode( Type[] obj ) => StructuralEqualityComparer<Type[]>.Instance.GetHashCode( obj );
	}*/

	public class StructuralEqualityComparer<T> : IEqualityComparer<T>
	{
		readonly IEqualityComparer comparer;
		public static StructuralEqualityComparer<T> Instance { get; } = new StructuralEqualityComparer<T>();
		StructuralEqualityComparer() : this( StructuralComparisons.StructuralEqualityComparer ) {}

		public StructuralEqualityComparer( IEqualityComparer comparer )
		{
			this.comparer = comparer;
		}

		public bool Equals( T x, T y ) => comparer.Equals( x, y );

		public int GetHashCode( T obj ) => comparer.GetHashCode( obj );
	}

	class CachedDelegateInvoker : IDelegateInvoker
	{
		readonly Func<object[], object> inner;
		readonly ConcurrentDictionary<object[], object> cache;

		public CachedDelegateInvoker( IDelegateInvoker inner ) : this( inner.ToDelegate(), new ConcurrentDictionary<object[], object>( StructuralEqualityComparer<object[]>.Instance ) ) {}

		public CachedDelegateInvoker( Func<object[], object> inner, ConcurrentDictionary<object[], object> cache )
		{
			this.inner = inner;
			this.cache = cache;
		}

		public object Invoke( object[] arguments ) => cache.GetOrAdd( arguments, inner );
	}

	public abstract class DelegateInvokerBase<T> : IDelegateInvoker
	{
		readonly int numberOfArguments;

		protected DelegateInvokerBase( T @delegate ) : this( @delegate, @delegate.AsValid<Delegate>().GetMethodInfo().GetParameterTypes().Length ) {}

		DelegateInvokerBase( T @delegate, int numberOfArguments )
		{
			Delegate = @delegate;
			this.numberOfArguments = numberOfArguments;
		}

		protected T Delegate { get; }

		object IDelegateInvoker.Invoke( object[] arguments )
		{
			if ( !Validate( arguments ) )
			{
				throw new ArgumentException( "Provided arguments are not valid for this invoker." );
			}

			return Invoke( arguments );
		}

		protected virtual bool Validate( object[] arguments ) => arguments.Length == numberOfArguments;

		protected abstract object Invoke( object[] arguments );
	}

	public abstract class CommandInvokerBase<T> : DelegateInvokerBase<T>
	{
		protected CommandInvokerBase( T @delegate ) : base( @delegate ) {}

		protected sealed override object Invoke( object[] arguments )
		{
			Execute( arguments );
			return null;
		}

		protected abstract void Execute( object[] arguments );
	}

	public class ActionInvoker : CommandInvokerBase<Action>
	{
		public ActionInvoker( Action action ) : base( action ) {}

		protected override void Execute( object[] arguments ) => Delegate();
	}

	public class ActionInvoker<T> : CommandInvokerBase<Action<T>>
	{
		public ActionInvoker( Action<T> action ) : base( action ) {}

		protected override bool Validate( object[] arguments ) => base.Validate( arguments ) && arguments[0] is T;

		protected override void Execute( object[] arguments ) => Delegate( (T)arguments[0] );
	}

	public class ActionInvoker<T1, T2> : CommandInvokerBase<Action<T1, T2>>
	{
		public ActionInvoker( Action<T1, T2> @delegate ) : base( @delegate ) {}

		protected override bool Validate( object[] arguments ) => base.Validate( arguments ) && arguments[0] is T1 && arguments[1] is T2;

		protected override void Execute( object[] arguments ) => Delegate( (T1)arguments[0], (T2)arguments[1] );
	}

	public class ActionInvoker<T1, T2, T3> : CommandInvokerBase<Action<T1, T2, T3>>
	{
		public ActionInvoker( Action<T1, T2, T3> @delegate ) : base( @delegate ) {}

		protected override bool Validate( object[] arguments ) => base.Validate( arguments ) && arguments[0] is T1 && arguments[1] is T2 && arguments[2] is T3;

		protected override void Execute( object[] arguments ) => Delegate( (T1)arguments[0], (T2)arguments[1], (T3)arguments[2] );
	}

	public class FactoryInvoker<T> : DelegateInvokerBase<Func<T>>
	{
		public FactoryInvoker( Func<T> @delegate ) : base( @delegate ) {}

		protected override object Invoke( object[] arguments ) => Delegate();
	}

	public class FactoryInvoker<T1, T> : DelegateInvokerBase<Func<T1, T>>
	{
		public FactoryInvoker( Func<T1, T> @delegate ) : base( @delegate ) {}

		protected override bool Validate( object[] arguments ) => base.Validate( arguments ) && arguments[0] is T1;

		protected override object Invoke( object[] arguments ) => Delegate( (T1)arguments[0] );
	}

	public class FactoryInvoker<T1, T2, T> : DelegateInvokerBase<Func<T1, T2, T>>
	{
		public FactoryInvoker( Func<T1, T2, T> @delegate ) : base( @delegate ) {}

		protected override bool Validate( object[] arguments ) => base.Validate( arguments ) && arguments[0] is T1 && arguments[1] is T2;

		protected override object Invoke( object[] arguments ) => Delegate( (T1)arguments[0], (T2)arguments[1] );
	}

	public class FactoryInvoker<T1, T2, T3, T> : DelegateInvokerBase<Func<T1, T2, T3, T>>
	{
		public FactoryInvoker( Func<T1, T2, T3, T> @delegate ) : base( @delegate ) {}

		protected override bool Validate( object[] arguments ) => base.Validate( arguments ) && arguments[0] is T1 && arguments[1] is T2 && arguments[2] is T3;

		protected override object Invoke( object[] arguments ) => Delegate( (T1)arguments[0], (T2)arguments[1], (T3)arguments[2] );
	}

	public class Delegates : Cache<object, ICache<MethodInfo, Delegate>>
	{
		public static Delegates Default { get; } = new Delegates();
		Delegates() : base( o => new Cache<MethodInfo, Delegate>( new Factory( o ).Create ) ) {}

		/*public Delegate Lookup( Delegate source )
		{
			if ( Contains( source.Target ) )
			{
				var inner = Get( source.Target );
				var method = source.GetMethodInfo();
				if ( inner.Contains( method ) )
				{
					return inner.Get( method );
				}
			}
			return null;
		}*/

		/*public T From<T>( T source ) where T : class
		{
			var @delegate = source as Delegate;
			if ( @delegate != null )
			{
				var inner = Get( @delegate.Target );
				var methodInfo = @delegate.GetMethodInfo();
				var contains = inner.Contains( methodInfo );
				var result = contains ? inner.Get( methodInfo ) : inner.SetValue( methodInfo, @delegate );
				return result as T;
			}
			return default(T);
		}*/

		sealed class Factory : FactoryBase<MethodInfo, Delegate>
		{
			readonly object instance;

			public Factory( object instance )
			{
				this.instance = instance;
			}

			public override Delegate Create( MethodInfo parameter )
			{
				var info = parameter.AccountForClosedDefinition( instance.GetType() );
				var delegateType = DelegateType.Default.Get( info );
				var result = info.CreateDelegate( delegateType, parameter.IsStatic ? null : instance );
				return result;
			}
		}
	}

	public class Invokers : Cache<object, ICache<MethodInfo, IDelegateInvoker>>
	{
		public static Invokers Default { get; } = new Invokers();
		Invokers() : base( o => new Cache<MethodInfo, IDelegateInvoker>( new Factory( o ).Create ) ) {}

		sealed class Factory : FactoryBase<MethodInfo, IDelegateInvoker>
		{
			readonly object instance;
			public Factory( object instance )
			{
				this.instance = instance;
			}

			public override IDelegateInvoker Create( MethodInfo parameter ) => Invoker.Default.Get( Delegates.Default.Get( instance ).Get( parameter ) );
		}
	}

	class Invoker : FactoryBase<Delegate, IDelegateInvoker>
	{
		public static ICache<Delegate, IDelegateInvoker> Default { get; } = new Invoker().ToCache();

		readonly static IDictionary<Type, Type> Mappings = new Dictionary<Type, Type>
														   {
															   { typeof(Action), typeof(ActionInvoker) },
															   { typeof(Action<>), typeof(ActionInvoker<>) },
															   { typeof(Action<,>), typeof(ActionInvoker<,>) },
															   { typeof(Action<,,>), typeof(ActionInvoker<,,>) },
															   { typeof(Func<>), typeof(FactoryInvoker<>) },
															   { typeof(Func<,>), typeof(FactoryInvoker<,>) },
															   { typeof(Func<,,>), typeof(FactoryInvoker<,,>) },
															   { typeof(FactoryInvoker<,,,>), typeof(FactoryInvoker<,,,>) }
														   }.ToImmutableDictionary();

		public override IDelegateInvoker Create( Delegate parameter )
		{
			var delegateType = parameter.GetType();
			var mapped = Mappings[delegateType.IsConstructedGenericType ? delegateType.GetGenericTypeDefinition() : delegateType];
			var type = mapped.GetTypeInfo().IsGenericTypeDefinition ? mapped.MakeGenericType( delegateType.GenericTypeArguments ) : mapped;

			var result = Constructor.Instance.Create<IDelegateInvoker>( new ConstructTypeRequest( type, parameter ) );
			return result;
		}
	}

	class DelegateType : Cache<MethodInfo, Type>
	{
		public static DelegateType Default { get; } = new DelegateType();
		DelegateType() : base( info => Expression.GetDelegateType( info.GetParameterTypes().Append( info.ReturnType ).Fixed() ) ) {}
	}

	class DelegateStack : AmbientStack<IDelegateInvoker>
	{
		public new static DelegateStack Default { get; } = new DelegateStack();
	}

	class DelegateContext<T> : Cache<IDelegateInvoker, T> where T : class
	{
		public static DelegateContext<T> Instance { get; } = new DelegateContext<T>();

		public T Current() => Get( DelegateStack.Default.GetCurrentItem() );

		public IDelegateInvoker Create( Delegate target, T context )
		{
			var core = Invoker.Default.Get( target );
			Set( core, context );
			var result = new ContextDelegateInvoker( core );
			return result;
		}
	}

	class ContextDelegateInvoker : IDelegateInvoker
	{
		readonly IDelegateInvoker invoker;
		public ContextDelegateInvoker( IDelegateInvoker invoker )
		{
			this.invoker = invoker;
		}

		public object Invoke( object[] arguments )
		{
			using ( DelegateStack.Default.Assignment( invoker ) )
			{
				return invoker.Invoke( arguments );
			}
		}
	}
}
