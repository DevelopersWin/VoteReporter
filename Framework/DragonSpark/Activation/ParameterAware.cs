using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DragonSpark.Activation
{
	public interface IArgumentCache<TArgument, TValue> : IAtomicCache<TArgument, TValue>
	{
		TValue GetOrSet( TArgument key, Func<TValue> factory );
	}

	public class ArgumentCache<TArgument, TValue> : ConcurrentDictionary<TArgument, TValue>, IArgumentCache<TArgument, TValue>
	{
		readonly static IEqualityComparer<TArgument> EqualityComparer = typeof(TArgument).IsStructural() ? (IEqualityComparer<TArgument>)StructuralEqualityComparer<TArgument>.Instance : EqualityComparer<TArgument>.Default;

		readonly Func<TArgument, TValue> body;

		public ArgumentCache() : this( argument => default(TValue) ) {}

		public ArgumentCache( Func<TArgument, TValue> body ) : base( EqualityComparer )
		{
			this.body = body;
		}

		public virtual bool Contains( TArgument instance ) => ContainsKey( instance );

		public virtual bool Remove( TArgument instance )
		{
			TValue item;
			return TryRemove( instance, out item );
		}

		public virtual void Set( TArgument instance, TValue value ) => this[instance] = value;
		public virtual TValue Get( TArgument key ) => GetOrAdd( key, body );

		public virtual TValue GetOrSet( TArgument key, Func<TValue> factory )
		{
			TValue result;
			return TryGetValue( key, out result ) ? result : GetOrAdd( key, factory() );
		}

		public TValue GetOrSet( TArgument key, Func<TArgument, TValue> factory ) => GetOrAdd( key, factory );
	}
	
	/*class RegisteredCacheFactory<TKey, TValue> : FactoryBase<InstanceMethod, IArgumentCache<TKey, TValue>>
	{
		public static RegisteredCacheFactory<TKey, TValue> Instance { get; } = new RegisteredCacheFactory<TKey, TValue>();
		RegisteredCacheFactory() : this( ParameterHandlerRegistry.Instance ) {}

		readonly IParameterHandlerRegistry registry;

		public RegisteredCacheFactory( IParameterHandlerRegistry registry )
		{
			this.registry = registry;
		}

		public override IArgumentCache<TKey, TValue> Create( InstanceMethod parameter )
		{
			var result = new ArgumentCache<TKey, TValue>();
			registry.Register( parameter, new CacheParameterHandler<TKey, TValue>( result ) );
			return result;
		}
	}*/

	// public interface I

	public interface IParameterAwareHandler
	{
		bool Handles( object parameter );

		bool Handle( object parameter, out object handled );
	}

	/*class ParameterAwareHandler : IParameterAwareHandler
	{
		public static ParameterAwareHandler Instance { get; } = new ParameterAwareHandler();

		public bool Handles( object parameter ) => false;

		public bool Handle( object parameter, out object handled )
		{
			handled = null;
			return false;
		}
	}*/

	/*class CompositeParameterAwareHandler : ConcurrentDictionary<object, IParameterAwareHandler>, IParameterAwareHandler
	{
		readonly ImmutableArray<IParameterAwareHandler> handlers;

		public CompositeParameterAwareHandler( ImmutableArray<IParameterAwareHandler> handlers )
		{
			this.handlers = handlers;
		}

		public bool Handles( object parameter ) => GetHandler( parameter ) != null;
		public bool Handle( object parameter, out object handled )
		{
			var handler = GetHandler( parameter );
			if ( handler != null )
			{
				var result = handler.Handle( parameter, out handled );

				IParameterAwareHandler removed;
				TryRemove( parameter, out removed );
				return result;
			}
			
			handled = null;
			return false;
		}

		IParameterAwareHandler GetHandler( object parameter )
		{
			IParameterAwareHandler found;
			return TryGetValue( parameter, out found ) ? found : Store( parameter );
		}

		IParameterAwareHandler Store( object parameter )
		{
			foreach ( var handler in handlers )
			{
				if ( handler.Handles( parameter ) )
				{
					TryAdd( parameter, handler );
					return handler;
				}
			}
			return null;
		}
	}*/

	/*public interface IParameterHandlerRegistry
	{
		void Register( InstanceMethod instance, IParameterAwareHandler handler );

		IParameterAwareHandler For( InstanceMethod instance );
	}

	public struct InstanceMethod
	{
		public InstanceMethod( object instance, MethodBase method )
		{
			Instance = instance;
			Method = method;
		}

		public object Instance { get; }
		public MethodBase Method { get; }
	}*/

	/*class ParameterAwareHandler : IParameterAwareHandler
	{
		public static ParameterAwareHandler Instance { get; } = new ParameterAwareHandler();

		public bool Handles( object parameter )
		{
			return false;
		}

		public bool Handle( object parameter, out object handled )
		{
			handled = null;
			return false;
		}
	}*/

	/*class ParameterHandlerRegistry : ActivatedCache<ParameterHandlerRegistry.Inner>, IParameterHandlerRegistry
	{
		public new static IParameterHandlerRegistry Instance { get; } = new ParameterHandlerRegistry();

		public void Register( InstanceMethod instance, IParameterAwareHandler handler )
		{
			var list = Get( instance.Instance ).Get( instance.Method );
			lock ( list )
			{
				list.Add( handler );
			}
		}

		internal class Inner : ArgumentCache<MethodBase, ISet<IParameterAwareHandler>>
		{
			public Inner() : base( _ => new HashSet<IParameterAwareHandler>() ) {}
		}

		public IParameterAwareHandler For( InstanceMethod instance ) => new CompositeParameterAwareHandler( Get( instance.Instance ).Get( instance.Method ).ToImmutableArray() );
	}*/

	class CacheParameterHandler<TKey, TValue> : IParameterAwareHandler
	{
		readonly ICache<TKey, TValue> cache;

		public CacheParameterHandler( ICache<TKey, TValue> cache )
		{
			this.cache = cache;
		}

		public bool Handles( object parameter ) => parameter is TKey && cache.Contains( (TKey)parameter );
		public bool Handle( object parameter, out object handled )
		{
			var result = Handles( parameter );
			handled = result ? cache.Get( (TKey)parameter ) : default(TValue);
			return result;
		}
	}
}