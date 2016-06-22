using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DragonSpark.Runtime.Properties
{
	public abstract class ExecutionCachedStoreBase<T> : DeferredTargetCachedStore<object, T> where T : class
	{
		protected ExecutionCachedStoreBase() : this( Delegates<T>.Default ) {}
		protected ExecutionCachedStoreBase( Func<T> create ) : this( new Cache<T>( create.Wrap().ToDelegate() ) ) {}
		protected ExecutionCachedStoreBase( ICache<object, T> cache ) : this( Execution.GetCurrent, cache ) {}
		protected ExecutionCachedStoreBase( Func<object> instance, ICache<object, T> cache ) : this( instance, cache, Coercer<T>.Instance ) {}
		protected ExecutionCachedStoreBase( Func<object> instance, ICache<object, T> cache, ICoercer<T> coercer ) : base( instance, cache, coercer ) {}

		/*struct Context
		{
			readonly Func<T> create;
			public Context( Func<T> create )
			{
				this.create = create;
			}

			public T Create( object instance ) => create();
		}*/
	}

	public class Stack<T> : IStack<T>
	{
		readonly System.Collections.Generic.Stack<T> store;
		readonly Action<IStack<T>> onEmpty;
		public Stack() : this( Delegates<IStack<T>>.Empty ) {}

		public Stack( System.Collections.Generic.Stack<T> store ) : this( store, Delegates<IStack<T>>.Empty ) {}

		public Stack( Action<IStack<T>> onEmpty ) : this( new System.Collections.Generic.Stack<T>(), onEmpty ) {}

		public Stack( System.Collections.Generic.Stack<T> store, Action<IStack<T>> onEmpty )
		{
			this.store = store;
			this.onEmpty = onEmpty;
		}

		public bool Contains( T item ) => store.Contains( item );

		public ImmutableArray<T> All() => store.ToImmutableArray();

		public T Peek() => store.PeekOrDefault();

		public void Push( T item ) => store.Push( item );

		public T Pop()
		{
			var result = store.Pop();
			if ( !store.Any() )
			{
				onEmpty( this );
			}
			return result;
		}
	}

	public interface IPropertyRegistry<T>
	{
		void Register( object key, ICache<T> instance );
		void Clear( object key, object instance );
	}

	class PropertyRegistry<T> : IPropertyRegistry<T>
	{
		public static PropertyRegistry<T> Instance { get; } = new PropertyRegistry<T>();

		readonly ConditionalWeakTable<object, ICache<T>> cache = new ConditionalWeakTable<object, ICache<T>>();

		public void Register( object key, ICache<T> instance ) => cache.Add( key, instance );

		public void Clear( object key, object instance )
		{
			ICache<T> property;
			if ( cache.TryGetValue( key, out property ) )
			{
				property.Remove( instance );
			}
		}
	}

	public static class AmbientStack
	{
		public static object GetCurrentItem( [Required]Type type ) => typeof(AmbientStack).Adapt().GenericMethods.Invoke( nameof(GetCurrentItem), type.ToItem() );

		public static T GetCurrentItem<T>() => AmbientStack<T>.Default.GetCurrentItem();

		// public static object GetCurrent( [Required]Type type ) => typeof(AmbientStack).Adapt().Invoke( nameof(GetCurrent), type.ToItem() );

		// static ImmutableArray<T> List<T>() => GetCurrent<T>().All();

		/*public static IStack<T> GetCurrent<T>() => AmbientStackProperty<T>.Default.Get( Execution.Current );*/
	}

	public interface IStackStore<T> : IWritableStore<IStack<T>>
	{
		T GetCurrentItem();
	}

	public class AmbientStack<T> : ExecutionCachedStoreBase<IStack<T>>, IStackStore<T>
	{
		public static AmbientStack<T> Default { get; } = new AmbientStack<T>();

		public AmbientStack() : this( Execution.GetCurrent ) {}
		public AmbientStack( Func<object> host ) : this( host, AmbientStackCache<T>.Default ) {}
		public AmbientStack( Func<object> host, ICache<object, IStack<T>> cache ) : base( host, cache ) {}

		public T GetCurrentItem() => Value.Peek();

		public struct Assignment : IDisposable
		{
			readonly IStackStore<T> store;

			public Assignment( T item ) : this( Default, item ) {}

			public Assignment( IStackStore<T> store, T item )
			{
				this.store = store;
				store.Value.Push( item );
			}

			public void Dispose() => store.Value.Pop();
		}
	}

	public class AmbientStackCache<T> : StoreCache<IStack<T>>
	{
		public static AmbientStackCache<T> Default { get; } = new AmbientStackCache<T>();

		public AmbientStackCache() : this( PropertyRegistry<IStack<T>>.Instance ) {}

		protected AmbientStackCache( IPropertyRegistry<IStack<T>> registry ) : this( registry, new Store( registry.Clear ) ) {}

		protected AmbientStackCache( IPropertyRegistry<IStack<T>> registry, IFactory<object, IWritableStore<IStack<T>>> factory ) : base( new ThreadLocalStoreCache<IStack<T>>( factory.ToDelegate() ) )
		{
			registry.Register( factory, this );
		}

		public class Store : FactoryWithSpecificationBase<object, IWritableStore<IStack<T>>>
		{
			readonly Action<Store, object> callback;

			public Store( Action<Store, object> callback )
			{
				this.callback = callback;
			}

			public override IWritableStore<IStack<T>> Create( object instance ) => new Factory( this, instance ).Create();

			class Factory
			{
				readonly Store owner;
				readonly object instance;
				readonly ThreadLocalStore<IStack<T>> store;
				readonly ConcurrentDictionary<IStack<T>, bool> empty = new ConcurrentDictionary<IStack<T>, bool>();
				readonly ThreadLocal<IStack<T>> local;
				readonly Action<IStack<T>> onEmpty;
				readonly Func<IStack<T>, bool> isEmpty;

				public Factory( Store owner, object instance )
				{
					this.owner = owner;
					this.instance = instance;

					local = new ThreadLocal<IStack<T>>( New, true );
					store = new ThreadLocalStore<IStack<T>>( local ).Configured( false );
					onEmpty = OnEmpty;
					isEmpty = IsEmpty;
				}

				public IWritableStore<IStack<T>> Create() => store;

				IStack<T> New() => new Stack<T>( onEmpty );

				void OnEmpty( IStack<T> item )
				{
					if ( empty.TryAdd( item, true ) && local.Values.All( isEmpty ) )
					{
						empty.Clear();
						owner.Clear( instance );
						store.Dispose();
					}
				}

				bool IsEmpty( IStack<T> stack )
				{
					bool stored;
					return empty.ContainsKey( stack ) && empty.TryGetValue( stack, out stored ) && stored;
				}
			}

			void Clear( object instance ) => callback( this, instance );
		}
	}

	public class ThreadLocalStore<T> : WritableStore<T>
	{
		readonly ThreadLocal<T> local;

		public ThreadLocalStore( [Required]Func<T> create ) : this( new ThreadLocal<T>( create ) ) {}

		public ThreadLocalStore( ThreadLocal<T> local )
		{
			this.local = local;
		}

		public override void Assign( T item ) => local.Value = item;

		protected override T Get() => local.Value;

		protected override void OnDispose()
		{
			local.Dispose();
			
			base.OnDispose();
		}
	}

	/*public class EqualityReference<T> : ExecutionCachedStoreBase<ConcurrentDictionary<int, T>> where T : class
	{
		public T From( T instance ) => Value.GetOrAdd( instance.GetHashCode(), instance.ToFactory<int, T>().ToDelegate() );

		public EqualityReference() : base( new ActivatedCache<ConcurrentDictionary<int, T>>() ) {}

		// public EqualityReference( Func<object> instance, IAttachedProperty<object, ConcurrentDictionary<int, T>> cache, ICoercer<ConcurrentDictionary<int, T>> coercer ) : base( instance, cache, coercer ) {}
	}*/

	public class EqualityReference<T> : TransformerBase<T> where T : class
	{
		readonly WeakList<T> list = new WeakList<T>();

		T GetOrAdd( T item )
		{
			lock ( list )
			{
				var current = list.Introduce( item, tuple => Equals( tuple.Item1, tuple.Item2 ) ).SingleOrDefault();
				if ( current == null )
				{
					list.Add( item );
					return item;
				}
				return current;
			}
		}

		public override T Create( T parameter ) => GetOrAdd( parameter );
	}

	public class Condition : Condition<object>
	{
		public new static Condition Default { get; } = new Condition();
	}
	
	public class Condition<T> : Cache<T, ConditionMonitor> where T : class
	{
		public static Condition<T> Default { get; } = new Condition<T>();

		public Condition() : base( key => new ConditionMonitor() ) {}
	}
}