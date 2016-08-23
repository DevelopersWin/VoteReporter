﻿using DragonSpark.Activation;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DragonSpark.Specifications;

namespace DragonSpark.Sources.Parameterized.Caching
{
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
		public TInstance Default { get; }
		public TValue Value { get; }
		public AttachedPropertyChangedEventType Type { get; }
	}*/

	/*class StoreConverter<T> : Converter<T, T>
	{
		public StoreConverter( IWritableStore<T> store ) : base( arg => store., @from ) {}
	}*/

	/*class SelfConverter<T> : Converter<T, T>
	{
		public static SelfConverter<T> Default { get; } = new SelfConverter<T>();

		SelfConverter() : base( Default<T>.Self, Default<T>.Self ) {}
	}

	class TupleConverter<T> : Converter<T, Tuple<T>>
	{
		public static TupleConverter<T> Default { get; } = new TupleConverter<T>();

		TupleConverter() : base( arg => new Tuple<T>( arg ), tuple => tuple.Item1 ) {}
	}*/

	public class ThreadLocalSourceCache<TInstance, TResult> : WritableSourceCache<TInstance, TResult> where TInstance : class
	{
		readonly static Func<TInstance, IAssignableSource<TResult>> Create = Store.Default.Get;
		public ThreadLocalSourceCache() : this( Create ) {}

		public ThreadLocalSourceCache( Func<TResult> create ) : this( new Store( create ).Get ) {}

		public ThreadLocalSourceCache( Func<TInstance, IAssignableSource<TResult>> create ) : base( create ) {}

		class Store : ParameterizedSourceBase<TInstance, IAssignableSource<TResult>>
		{
			public static Store Default { get; } = new Store();

			readonly Func<TResult> create;

			Store() : this( () => default(TResult) ) {}

			public Store( Func<TResult> create )
			{
				this.create = create;
			}

			public override IAssignableSource<TResult> Get( TInstance instance ) => new ThreadLocalStore<TResult>( create );
		}
	}

	public class SetCache<TInstance, TItem> : Cache<TInstance, ISet<TItem>> where TInstance : class
	{
		public SetCache() : base( key => new HashSet<TItem>() ) {}
		public SetCache( Func<TInstance, ISet<TItem>> create ) : base( create ) {}
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

	public interface ICache<in TInstance, TValue> : IAssignableParameterizedSource<TInstance, TValue>
	{
		bool Contains( TInstance instance );
		
		bool Remove( TInstance instance );
	}

	/*public interface IConfigurableCache<T> : IConfigurableCache<object, T>, ICache<T> {}

	public interface IConfigurableCache<TInstance, TValue> : ICache<TInstance, TValue>, IAssignable<Func<TInstance, TValue>> {}*/

	public abstract class FactoryCache<TInstance, TValue> : DecoratedCache<TInstance, TValue>
	{
		readonly protected static ISpecification<TInstance> DefaultSpecification = Specifications<TInstance>.Always;

		protected FactoryCache() : this( DefaultSpecification ) {}
		protected FactoryCache( ISpecification<TInstance> specification ) : this( new ParameterizedScope<TInstance, TValue>( instance => default(TValue) ), specification ) {}

		FactoryCache( IParameterizedScope<TInstance, TValue> configuration, ISpecification<TInstance> specification ) : base( configuration.ToCache() )
		{
			var delegated = new DelegatedValidatedSource<TInstance, TValue>( Create, specification );
			var factory = specification == DefaultSpecification ? delegated : delegated.WithAutoValidation();
			configuration.Assign( new Func<TInstance, TValue>( factory.Get ).Wrap() );
		}

		protected abstract TValue Create( TInstance parameter );
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

	public class DecoratedSourceCache<TInstance, TValue> : CacheBase<TInstance, TValue> where TInstance : class
	{
		readonly ISourceCache<TInstance, TValue> inner;

		public DecoratedSourceCache() : this( instance => default(TValue) ) {}
		public DecoratedSourceCache( Func<TInstance, TValue> create ) : this( new WritableSourceCache<TInstance, TValue>( create ) ) {}

		public DecoratedSourceCache( ISourceCache<TInstance, TValue> inner )
		{
			this.inner = inner;
		}

		public override bool Contains( TInstance instance ) => inner.Contains( instance );

		public override void Set( TInstance instance, [Optional]TValue value ) => inner.Get( instance ).Assign( value );

		public override TValue Get( TInstance instance ) => inner.Get( instance ).Get();

		public override bool Remove( TInstance instance ) => inner.Remove( instance );
	}

	public class ActivatedCache<TInstance, TResult> : Cache<TInstance, TResult> where TInstance : class where TResult : class, new()
	{
		public static ActivatedCache<TInstance, TResult> Default { get; } = new ActivatedCache<TInstance, TResult>();
		public ActivatedCache() : base( instance => new TResult() ) {}
	}
}