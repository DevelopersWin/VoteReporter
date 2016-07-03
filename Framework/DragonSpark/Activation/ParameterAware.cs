using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Activation
{
	public interface IArgumentCache<in TArgument, TValue> : ICache<TArgument, TValue>
	{
		TValue GetOrSet( TArgument key, Func<TValue> factory );
	}

	public class ArgumentCache<TArgument, TValue> : CacheBase<TArgument, TValue>, IArgumentCache<TArgument, TValue>
	{
		readonly ConcurrentDictionary<TArgument, TValue> items;
		readonly Func<TArgument, TValue> body;

		public ArgumentCache() : this( argument => default(TValue) ) {}

		public ArgumentCache( Func<TArgument, TValue> body ) : this( body, new ConcurrentDictionary<TArgument, TValue>( typeof(TArgument).IsStructural() ? (IEqualityComparer<TArgument>)StructuralEqualityComparer<TArgument>.Instance : EqualityComparer<TArgument>.Default ) ) {}

		public ArgumentCache( Func<TArgument, TValue> body, ConcurrentDictionary<TArgument, TValue> items )
		{
			this.body = body;
			this.items = items;
		}

		public override bool Contains( TArgument instance ) => items.ContainsKey( instance );

		public override bool Remove( TArgument instance )
		{
			TValue item;
			return items.TryRemove( instance, out item );
		}

		public override void Set( TArgument instance, TValue value ) => items[instance] = value;
		public override TValue Get( TArgument key ) => items.GetOrAdd( key, body );

		public TValue GetOrSet( TArgument key, Func<TValue> factory )
		{
			TValue result;
			return items.TryGetValue( key, out result ) ? result : items.GetOrAdd( key, factory() );
		}
	}

	abstract class FromArgumentCacheFactoryBase<TKey, T> : FactoryBase<Delegate, T>
	{
		readonly static Func<Delegate, IArgumentCache<TKey, object>> DefaultCacheSource = DelegateReferenceCacheFactory<TKey, object>.Instance.Create;

		readonly Func<Delegate, IArgumentCache<TKey, object>> cacheSource;
		readonly Func<IArgumentCache<TKey, object>, T> factory;

		protected FromArgumentCacheFactoryBase( Func<IArgumentCache<TKey, object>, T> factory ) : this( DefaultCacheSource, factory ) {}

		protected FromArgumentCacheFactoryBase( Func<Delegate, IArgumentCache<TKey, object>> cacheSource, Func<IArgumentCache<TKey, object>, T> factory )
		{
			this.cacheSource = cacheSource;
			this.factory = factory;
		}

		public override T Create( Delegate parameter )
		{
			var cache = cacheSource( parameter );
			var result = factory( cache );
			return result;
		}
	}

	class DelegateReferenceCacheFactory<TKey, TValue> : FactoryBase<Delegate, IArgumentCache<TKey, TValue>>
	{
		public static DelegateReferenceCacheFactory<TKey, TValue> Instance { get; } = new DelegateReferenceCacheFactory<TKey, TValue>();
		DelegateReferenceCacheFactory() : this( DelegateParameterHandlerRegistry.Instance ) {}

		readonly IDelegateParameterHandlerRegistry registry;

		public DelegateReferenceCacheFactory( IDelegateParameterHandlerRegistry registry )
		{
			this.registry = registry;
		}

		public override IArgumentCache<TKey, TValue> Create( Delegate parameter )
		{
			var result = new ArgumentCache<TKey, TValue>();
			registry.Register( parameter, new CacheParameterHandler<TKey, TValue>( result ) );
			return result;
		}
	}

	public interface IParameterAwareHandler
	{
		bool Handles( object parameter );

		bool Handle( object parameter, out object handled );
	}

	/*public class ParameterHandlerAwareParameterValidationMonitor : IParameterValidationMonitor
	{
		readonly IParameterAwareHandler handler;
		readonly IParameterValidationMonitor inner;
		public ParameterHandlerAwareParameterValidationMonitor( IParameterAwareHandler handler, IParameterValidationMonitor inner )
		{
			this.handler = handler;
			this.inner = inner;
		}

		public bool IsValid( object parameter )
		{
			return handler.Handles( parameter ) || inner.IsValid( parameter );
		}

		public void MarkValid( object parameter, bool valid ) {}
		public void Clear( object parameter ) {}
	}*/

	class CompositeParameterAwareHandler : IParameterAwareHandler
	{
		readonly ICache<object, IParameterAwareHandler> store = new ArgumentCache<object, IParameterAwareHandler>();

		readonly ImmutableArray<IParameterAwareHandler> handlers;

		public CompositeParameterAwareHandler( ImmutableArray<IParameterAwareHandler> handlers )
		{
			this.handlers = handlers;
		}

		public bool Handles( object parameter ) => Get( parameter ) != null;
		public bool Handle( object parameter, out object handled )
		{
			var handler = Get( parameter );
			if ( handler != null )
			{
				var result = handler.Handle( parameter, out handled );
				store.Remove( parameter );
				return result;
			}
			
			handled = null;
			return false;
		}

		IParameterAwareHandler Get( object parameter ) => store.Get( parameter ) ?? Store( parameter );

		IParameterAwareHandler Store( object parameter )
		{
			foreach ( var handler in handlers )
			{
				if ( handler.Handles( parameter ) )
				{
					return store.SetValue( parameter, handler );
				}
			}
			return null;
		}
	}

	public interface IDelegateParameterHandlerRegistry
	{
		void Register( Delegate @delegate, IParameterAwareHandler handler );
	}

	class DelegateParameterHandlerRegistry : IDelegateParameterHandlerRegistry
	{
		public static IDelegateParameterHandlerRegistry Instance { get; } = new DelegateParameterHandlerRegistry();

		readonly ICache<Delegate, IList<IParameterAwareHandler>> handlers = new ListCache<Delegate, IParameterAwareHandler>();

		public void Register( Delegate @delegate, IParameterAwareHandler handler )
		{
			var list = handlers.Get( @delegate );
			lock ( @delegate )
			{
				list.Add( handler );
			}
		}
	}

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