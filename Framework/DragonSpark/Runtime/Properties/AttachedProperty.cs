using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Sources;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DragonSpark.Runtime.Properties
{
	public static class CacheExtensions
	{
		public static TValue SetValue<TInstance, TValue>( this IAssignableParameterizedSource<TInstance, TValue> @this, TInstance instance, TValue value )
		{
			@this.Set( instance, value );
			return value;
		}

		public static TValue SetOrClear<TInstance, TValue>( this ICache<TInstance, TValue> @this, TInstance instance, TValue value = default(TValue) )
		{
			if ( value.IsAssigned() )
			{
				@this.Set( instance, value );
			}
			else
			{
				@this.Remove( instance );
			}
			
			return value;
		}

		public static Assignment<T1, T2> Assignment<T1, T2>( this ICache<T1, T2> @this, T1 first, T2 second )  => new Assignment<T1, T2>( new CacheAssign<T1, T2>( @this ), Assignments.From( first ), new Value<T2>( second ) );

		public static ImmutableArray<TResult> GetMany<TParameter, TResult>( this ICache<TParameter, TResult> @this, ImmutableArray<TParameter> parameters, Func<TResult, bool> where = null ) =>
			parameters
				.Select( @this.ToDelegate() )
				.Where( @where ?? Where<TResult>.Assigned ).ToImmutableArray();
		

		public static Func<TInstance, TValue> ToDelegate<TInstance, TValue>( this ICache<TInstance, TValue> @this ) => DelegateCache<TInstance, TValue>.Default.Get( @this );
		class DelegateCache<TInstance, TValue> : Cache<ICache<TInstance, TValue>, Func<TInstance, TValue>>
		{
			public static DelegateCache<TInstance, TValue> Default { get; } = new DelegateCache<TInstance, TValue>();

			DelegateCache() : base( command => command.Get ) {}
		}

		public static IScope<T> Scoped<T>( this IParameterizedSource<object, T> @this ) => @this.ToDelegate().Scoped();
		public static IScope<T> Scoped<T>( this Func<object, T> @this ) => Scopes<T>.Default.Get( @this );
		class Scopes<T> : Cache<Func<object, T>, IScope<T>>
		{
			public static Scopes<T> Default { get; } = new Scopes<T>();
			Scopes() : base( cache => new CachedScope<T>( cache ) ) {}
		}

		/*public static TDelegate Apply<TContext, TDelegate>( this ICache<TDelegate, TContext> @this, TDelegate source, TContext context ) where TDelegate : class
		{
			@this.Set( source, context );
			var result = Invocation.Create( source );
			return result;
		}

		public static TContext Context<TContext, TDelegate>( this ICache<TDelegate, TContext> @this ) where TDelegate : class
		{
			var instance = Invocation.GetCurrent() as TDelegate;
			var result = instance != null ? @this.Get( instance ) : default(TContext);
			return result;
		}*/
	}

	/*public enum AttachedPropertyChangedEventType
	{
		Set, Clear
	}

	public struct AttachedPropertyChangedEvent<TInstance, TValue> where TInstance : class
	{
		public AttachedPropertyChangedEvent( IAttachedProperty<TInstance, TValue> instance, TInstance instance, TValue value = default(TValue), AttachedPropertyChangedEventType type = AttachedPropertyChangedEventType.Clear )
		{
			Property = instance;
			Instance = instance;
			Value = value;
			Type = type;
		}

		public IAttachedProperty<TInstance, TValue> Property { get; }
		public TInstance Instance { get; }
		public TValue Value { get; }
		public AttachedPropertyChangedEventType Type { get; }
	}*/

	/*class StoreConverter<T> : Converter<T, T>
	{
		public StoreConverter( IWritableStore<T> store ) : base( arg => store., @from ) {}
	}*/

	/*class SelfConverter<T> : Converter<T, T>
	{
		public static SelfConverter<T> Instance { get; } = new SelfConverter<T>();

		SelfConverter() : base( Default<T>.Self, Default<T>.Self ) {}
	}

	class TupleConverter<T> : Converter<T, Tuple<T>>
	{
		public static TupleConverter<T> Instance { get; } = new TupleConverter<T>();

		TupleConverter() : base( arg => new Tuple<T>( arg ), tuple => tuple.Item1 ) {}
	}*/

	public class ThreadLocalStoreCache<T> : ThreadLocalStoreCache<object, T>
	{
		public ThreadLocalStoreCache() {}
		public ThreadLocalStoreCache( Func<T> create ) : base( create ) {}

		public ThreadLocalStoreCache( Func<object, IWritableStore<T>> create ) : base( create ) {}
		/*public ThreadLocalStoreCache() : this( () => default(T) ) {}
		public ThreadLocalStoreCache( Func<T> create ) : base( create ) {}

		protected ThreadLocalStoreCache( IAttachedPropertyStore<object, T> store ) : base( store ) {}*/

		// protected ThreadLocalAttachedProperty( Func<object, IWritableStore<T>> store ) : base( store ) {}
	}

	public class ThreadLocalStoreCache<TInstance, TResult> : WritableStoreCache<TInstance, TResult> where TInstance : class
	{
		readonly static Func<TInstance, IWritableStore<TResult>> Create = Store.Instance.Create;
		public ThreadLocalStoreCache() : this( Create ) {}

		public ThreadLocalStoreCache( Func<TResult> create ) : this( new Store( create ).Create ) {}

		public ThreadLocalStoreCache( Func<TInstance, IWritableStore<TResult>> create ) : base( create ) {}

		class Store : FactoryBase<TInstance, IWritableStore<TResult>>
		{
			public static Store Instance { get; } = new Store();

			readonly Func<TResult> create;

			Store() : this( () => default(TResult) ) {}

			public Store( Func<TResult> create ) : base( Specifications<TInstance>.Always )
			{
				this.create = create;
			}

			public override IWritableStore<TResult> Create( TInstance instance ) => new ThreadLocalStore<TResult>( create );
		}
	}

	public class SetCache<T> : SetCache<object, T>, ICache<ISet<T>>
	{
		public SetCache() {}
		public SetCache( Func<object, ISet<T>> create ) : base( create ) {}
	}

	public class SetCache<TInstance, TItem> : Cache<TInstance, ISet<TItem>> where TInstance : class
	{
		public SetCache() : base( key => new HashSet<TItem>() ) {}
		public SetCache( Func<TInstance, ISet<TItem>> create ) : base( create ) {}
	}

	public class ListCache : ListCache<object>
	{
		public static ListCache Default { get; } = new ListCache();

		public ListCache() {}
		public ListCache( Func<object, IList<object>> create ) : base( create ) {}
	}

	public class ListCache<T> : ListCache<object, T>, ICache<IList<T>>
	{
		// public static ListCache Default { get; } = new ListCache();

		public ListCache() {}
		public ListCache( Func<object, IList<T>> create ) : base( create ) {}
	}
	
	public class ListCache<TInstance, TItem> : Cache<TInstance, IList<TItem>> where TInstance : class
	{
		/*public static ListCache Default { get; } = new ListCache();*/

		public ListCache() : base( key => new List<TItem>() ) {}
		public ListCache( Func<TInstance, IList<TItem>> create ) : base( create ) {}
	}

	
	public interface ICache<T> : ICache<object, T>, IAssignableParameterizedSource<T> {}
	public interface ICache<in TInstance, TValue> : IAssignableParameterizedSource<TInstance, TValue>
	{
		bool Contains( TInstance instance );
		
		bool Remove( TInstance instance );
	}

	/*public interface IConfigurableCache<T> : IConfigurableCache<object, T>, ICache<T> {}

	public interface IConfigurableCache<TInstance, TValue> : ICache<TInstance, TValue>, IAssignable<Func<TInstance, TValue>> {}*/

	public static class CacheFactory
	{
		public static ICache<T> Create<T>( Func<T> parameter ) => Create( parameter.Wrap() );

		public static ICache<T> Create<T>( Func<object, T> parameter ) => Implementations<T>.Factory( parameter );

		public static ICache<TInstance, TValue> Create<TInstance, TValue>( Func<TInstance, TValue> parameter ) => Implementations<TInstance, TValue>.Factory( parameter );

		static class Implementations<T>
		{
			public static Func<Func<object, T>, ICache<T>> Factory { get; } = Create();

			static Func<Func<object, T>, ICache<T>> Create()
			{
				var definition = typeof(T).GetTypeInfo().IsValueType ? typeof(StoreCache<>) : typeof(Cache<>);
				var generic = definition.MakeGenericType( typeof(T) );
				var result = ParameterConstructor<Func<object, T>, ICache<T>>.Make( typeof(Func<object, T>), generic );
				return result;
			}
		}

		static class Implementations<TInstance, TValue>
		{
			public static Func<Func<TInstance, TValue>, ICache<TInstance, TValue>> Factory { get; } = Create();

			static Func<Func<TInstance, TValue>, ICache<TInstance, TValue>> Create()
			{
				var definition = typeof(TValue).GetTypeInfo().IsValueType ? typeof(StoreCache<,>) : typeof(Cache<,>);
					var generic = definition.MakeGenericType( typeof(TInstance), typeof(TValue) );
					var result = ParameterConstructor<Func<TInstance, TValue>, ICache<TInstance, TValue>>.Make( typeof(Func<TInstance, TValue>), generic );
					return result;
			}
		}
	}
	
	public abstract class FactoryCache<T> : FactoryCache<object, T>, ICache<T>
	{
		protected FactoryCache() : this( DefaultSpecification ) {}
		protected FactoryCache( ISpecification<object> specification ) : base( specification ) {}
	}

	public abstract class FactoryCache<TInstance, TValue> : DecoratedCache<TInstance, TValue>
	{
		readonly protected static ISpecification<TInstance> DefaultSpecification = Specifications<TInstance>.Always;

		protected FactoryCache() : this( DefaultSpecification ) {}
		protected FactoryCache( ISpecification<TInstance> specification ) : this( new ParameterizedScope<TInstance, TValue>( instance => default(TValue) ), specification ) {}

		FactoryCache( IParameterizedScope<TInstance, TValue> configuration, ISpecification<TInstance> specification ) : base( configuration.ToCache() )
		{
			var delegated = new DelegatedFactory<TInstance, TValue>( Create, specification );
			var factory = specification == DefaultSpecification ? delegated : delegated.WithAutoValidation();
			configuration.Assign( new Func<TInstance, TValue>( factory.Create ).Wrap() );
			// configuration.Assigned( factory.Create );
		}

		protected abstract TValue Create( TInstance parameter );
	}

	public class DecoratedCache<T> : DecoratedCache<object, T>
	{
		public DecoratedCache( Func<object, T> factory ) : base( factory ) {}
		public DecoratedCache( ICache<object, T> cache ) : base( cache ) {}
	}

	public class DecoratedCache<TInstance, TValue> : CacheBase<TInstance, TValue>
	{
		readonly ICache<TInstance, TValue> cache;
		public DecoratedCache() : this( ParameterConstructor<TInstance, TValue>.Default ) {}

		public DecoratedCache( Func<TInstance, TValue> factory ) : this( CacheFactory.Create( factory ) ) {}

		public DecoratedCache( ICache<TInstance, TValue> cache )
		{
			this.cache = cache;
		}

		public override TValue Get( TInstance parameter ) => cache.Get( parameter );

		public override bool Contains( TInstance instance ) => cache.Contains( instance );

		public override bool Remove( TInstance instance ) => cache.Remove( instance );

		public override void Set( TInstance instance, TValue value ) => cache.Set( instance, value );
	}

	public class Cache<T> : Cache<object, T>, ICache<T>/*, IConfigurableCache<T>*/ where T : class
	{
		public Cache() {}
		public Cache( Func<object, T> create ) : base( create ) {}
	}

	public class EqualityReferenceCache<TInstance, TValue> : DecoratedCache<TInstance, TValue> where TInstance : class
	{
		readonly static Func<TInstance, TInstance> DefaultSource = EqualityReference<TInstance>.Instance.Get;

		readonly Func<TInstance, TInstance> equalitySource;

		public EqualityReferenceCache() : this( instance => default(TValue) ) {}
		public EqualityReferenceCache( Func<TInstance, TValue> create ) : this( create, DefaultSource ) {}
		public EqualityReferenceCache( Func<TInstance, TValue> create , Func<TInstance, TInstance> equalitySource ) : this( CacheFactory.Create( create ), equalitySource ) {}

		public EqualityReferenceCache( ICache<TInstance, TValue> inner, Func<TInstance, TInstance> equalitySource ) : base( inner )
		{
			this.equalitySource = equalitySource;
		}

		public override bool Contains( TInstance instance ) => base.Contains( equalitySource( instance ) );

		public override bool Remove( TInstance instance ) => base.Remove( equalitySource( instance ) );

		public override void Set( TInstance instance, [Optional]TValue value ) => base.Set( equalitySource( instance ), value );

		public override TValue Get( TInstance instance ) => base.Get( equalitySource( instance )  );
	}


	public interface IAtomicCache<TArgument, TValue> : ICache<TArgument, TValue>
	{
		TValue GetOrSet( TArgument key, Func<TArgument, TValue> factory );
	}

	public class Cache<TInstance, TValue> : CacheBase<TInstance, TValue>, IAtomicCache<TInstance, TValue> where TInstance : class where TValue : class
	{
		readonly ConditionalWeakTable<TInstance, TValue>.CreateValueCallback create;

		readonly ConditionalWeakTable<TInstance, TValue> items = new ConditionalWeakTable<TInstance, TValue>();

		public Cache() : this( new Func<TInstance, TValue>( instance => default(TValue) ) ) {}

		public Cache( Func<TInstance, TValue> create ) : this( new ConditionalWeakTable<TInstance, TValue>.CreateValueCallback( create ) ) {}

		Cache( ConditionalWeakTable<TInstance, TValue>.CreateValueCallback create )
		{
			this.create = create;
		}

		public override bool Contains( TInstance instance )
		{
			TValue temp;
			return items.TryGetValue( instance, out temp );
		}

		public override void Set( TInstance instance, [Optional]TValue value )
		{
			lock ( items )
			{
				items.Remove( instance );
				items.Add( instance, value );
			}
		}

		public override TValue Get( TInstance instance ) => items.GetValue( instance, create );
		
		public override bool Remove( TInstance instance ) => items.Remove( instance );

		public TValue GetOrSet( TInstance instance, Func<TInstance, TValue> factory ) => items.GetValue( instance, new ConditionalWeakTable<TInstance, TValue>.CreateValueCallback( factory ) );
	}

	public abstract class CacheBase<TInstance, TValue> : AssignableParameterizedSourceBase<TInstance, TValue>, ICache<TInstance, TValue>
	{
		public abstract bool Contains( TInstance instance );
		public abstract bool Remove( TInstance instance );
	}

	public class StoreCache<T> : StoreCache<object, T>, ICache<T>
	{
		public StoreCache() : this( new WritableStoreCache<object, T>() ) {}
		public StoreCache( Func<object, T> create ) : this( new WritableStoreCache<object, T>( create ) ) {}

		public StoreCache( IStoreCache<object, T> inner ) : base( inner ) {}
	}

	public class StoreCache<TInstance, TValue> : CacheBase<TInstance, TValue> where TInstance : class
	{
		readonly IStoreCache<TInstance, TValue> inner;

		public StoreCache() : this( instance => default(TValue) ) {}
		public StoreCache( Func<TInstance, TValue> create ) : this( new WritableStoreCache<TInstance, TValue>( create ) ) {}

		public StoreCache( IStoreCache<TInstance, TValue> inner )
		{
			this.inner = inner;
		}

		public override bool Contains( TInstance instance ) => inner.Contains( instance );

		public override void Set( TInstance instance, [Optional]TValue value ) => inner.Get( instance ).Assign( value );

		public override TValue Get( TInstance instance ) => inner.Get( instance ).Value;

		public override bool Remove( TInstance instance ) => inner.Remove( instance );
	}

	public interface IStoreCache<in TInstance, TValue> : ICache<TInstance, IWritableStore<TValue>> {}

	public class WritableStoreCache<TInstance, TValue> : Cache<TInstance, IWritableStore<TValue>>, IStoreCache<TInstance, TValue> where TInstance : class
	{
		public WritableStoreCache() : this( instance => new FixedStore<TValue>() ) {}

		public WritableStoreCache( Func<TInstance, TValue> create ) : this( new Func<TInstance, IWritableStore<TValue>>( new Context( create ).Create ) ) {}

		public WritableStoreCache( Func<TInstance, IWritableStore<TValue>> create ) : base( create ) {}

		class Context
		{
			readonly Func<TInstance, TValue> create;
			public Context( Func<TInstance, TValue> create )
			{
				this.create = create;
			}

			public IWritableStore<TValue> Create( TInstance instance ) => new FixedStore<TValue>( create( instance ) );
		}
	}

	public class ActivatedCache<T> : ActivatedCache<object, T>, ICache<T> where T : class, new()
	{
		public new static ActivatedCache<T> Instance { get; } = new ActivatedCache<T>();
		public ActivatedCache() {}
	}

	public class ActivatedCache<TInstance, TResult> : Cache<TInstance, TResult> where TInstance : class where TResult : class, new()
	{
		public static ActivatedCache<TInstance, TResult> Instance { get; } = new ActivatedCache<TInstance, TResult>();
		public ActivatedCache() : base( instance => new TResult() ) {}
	}
}