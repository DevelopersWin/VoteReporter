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
		readonly static IGenericMethodContext<Invoke> Method = typeof(AmbientStack).Adapt().GenericFactoryMethods[nameof(GetCurrentItem)];

		public static object GetCurrentItem( [Required]Type type ) => Method.Make( type ).Invoke<object>();

		public static T GetCurrentItem<T>() => AmbientStack<T>.Default.GetCurrentItem();

		public static StackAssignment<T> Assignment<T>( this IStackSource<T> @this, T item )  => new StackAssignment<T>( @this, item );

		public struct StackAssignment<T> : IDisposable
		{
			readonly IStackSource<T> source;

			// public StackAssignment( T item ) : this( AmbientStack<T>.Default, item ) {}

			public StackAssignment( IStackSource<T> source, T item )
			{
				this.source = source;
				source.Get().Push( item );
			}

			public void Dispose() => source.Get().Pop();
		}
	}

	public interface IStackSource<T> : ISource<IStack<T>>
	{
		T GetCurrentItem();
	}

	public class AmbientStack<T> : ExecutionScope<IStack<T>>, IStackSource<T>
	{
		public static AmbientStack<T> Default { get; } = new AmbientStack<T>();
		/*readonly static Func<IStack<T>> Store = Default.Get;*/

		public AmbientStack() : this( AmbientStackCache<T>.Default ) {}
		public AmbientStack( ICache<IStack<T>> cache ) : base( cache ) {}

		public T GetCurrentItem() => Value.Peek();
	}

	public class AmbientStackCache<T> : StoreCache<IStack<T>>
	{
		public static AmbientStackCache<T> Default { get; } = new AmbientStackCache<T>();

		public AmbientStackCache() : this( PropertyRegistry<IStack<T>>.Instance ) {}

		protected AmbientStackCache( IPropertyRegistry<IStack<T>> registry ) : this( registry, new Store( registry.Clear ) ) {}

		protected AmbientStackCache( IPropertyRegistry<IStack<T>> registry, IParameterizedSource<object, IWritableStore<IStack<T>>> factory ) : base( new ThreadLocalStoreCache<IStack<T>>( factory.Get ) )
		{
			registry.Register( factory, this );
		}

		sealed class Store : FactoryBase<object, IWritableStore<IStack<T>>>
		{
			readonly Action<Store, object> callback;

			public Store( Action<Store, object> callback )
			{
				this.callback = callback;
			}

			public override IWritableStore<IStack<T>> Create( object instance ) => new Factory( this, instance ).Create();

			sealed class Factory
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
					store = new ThreadLocalStore<IStack<T>>( local );
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

		public ThreadLocalStore() : this( () => default(T) ) {}

		public ThreadLocalStore( Func<T> create ) : this( new ThreadLocal<T>( create ) ) {}

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

	public class EqualityReference<T> : TransformerBase<T> where T : class
	{
		public static EqualityReference<T> Instance { get; } = new EqualityReference<T>();
		EqualityReference() {}

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

		public override T Get( T parameter ) => GetOrAdd( parameter );
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