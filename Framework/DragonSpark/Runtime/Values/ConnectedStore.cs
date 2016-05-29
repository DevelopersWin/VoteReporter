using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DragonSpark.Runtime.Values
{
	public class Stack<T> : IStack<T>
	{
		readonly System.Collections.Generic.Stack<T> store;
		readonly Action<IStack<T>> onEmpty;
		public Stack() : this( Default<IStack<T>>.Empty ) {}

		public Stack( System.Collections.Generic.Stack<T> store ) : this( store, Default<IStack<T>>.Empty ) {}

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
		void Register( object key, IAttachedProperty<T> instance );
		void Clear( object key, object instance );
	}

	class PropertyRegistry<T> : IPropertyRegistry<T>
	{
		public static PropertyRegistry<T> Instance { get; } = new PropertyRegistry<T>();

		readonly ConditionalWeakTable<object, IAttachedProperty<T>> cache = new ConditionalWeakTable<object, IAttachedProperty<T>>();

		public void Register( object key, IAttachedProperty<T> instance ) => cache.Add( key, instance );

		public void Clear( object key, object instance )
		{
			IAttachedProperty<T> property;
			if ( cache.TryGetValue( key, out property ) )
			{
				property.Clear( instance );
			}
		}
	}

	public static class AmbientStack
	{
		public static object GetCurrentItem( [Required]Type type ) => typeof(AmbientStack).Adapt().Invoke( nameof(GetCurrentItem), type.ToItem() );

		public static T GetCurrentItem<T>() => GetCurrent<T>().Peek();

		// public static object GetCurrent( [Required]Type type ) => typeof(AmbientStack).Adapt().Invoke( nameof(GetCurrent), type.ToItem() );

		// static ImmutableArray<T> List<T>() => GetCurrent<T>().All();

		public static IStack<T> GetCurrent<T>() => AmbientStackProperty<T>.Default.Get( Execution.Current );
	}

	public class AmbientStackProperty<T> : ThreadLocalAttachedProperty<IStack<T>>
	{
		public static AmbientStackProperty<T> Default { get; } = new AmbientStackProperty<T>();

		public AmbientStackProperty() : this( PropertyRegistry<IStack<T>>.Instance ) {}

		protected AmbientStackProperty( IPropertyRegistry<IStack<T>> registry ) : this( registry, new Store( registry.Clear ) ) {}

		protected AmbientStackProperty( IPropertyRegistry<IStack<T>> registry, IAttachedPropertyStore<object, IStack<T>> store ) : base( store )
		{
			registry.Register( store, this );
		}

		public class Store : AttachedPropertyStoreBase<object, IStack<T>>
		{
			readonly Action<Store, object> callback;

			public Store( Action<Store, object> callback )
			{
				this.callback = callback;
			}

			public override IWritableStore<IStack<T>> Create( object instance ) => new Factory( this, instance ).Create();

			class Factory : Disposable
			{
				readonly Store owner;
				readonly object instance;
				readonly ThreadLocal<IStack<T>> local;
				readonly ConcurrentDictionary<IStack<T>, bool> empty = new ConcurrentDictionary<IStack<T>, bool>();

				public Factory( Store owner, object instance )
				{
					this.owner = owner;
					this.instance = instance;
					local = new ThreadLocal<IStack<T>>( New, true );
				}

				public IWritableStore<IStack<T>> Create()
				{
					var result = new ThreadLocalStore<IStack<T>>( local );
					this.AssociateForDispose( result );
					return result;
				}

				IStack<T> New() => new Stack<T>( OnEmpty );

				void OnEmpty( IStack<T> item )
				{
					if ( empty.TryAdd( item, true ) && local.Values.All( IsEmpty ) )
					{
						empty.Clear();
						Dispose();
					}
				}

				bool IsEmpty( IStack<T> stack )
				{
					bool isEmpty;
					return empty.ContainsKey( stack ) && empty.TryGetValue( stack, out isEmpty ) && isEmpty;
				}

				protected override void OnDispose( bool disposing ) => owner.Clear( instance );
			}

			void Clear( object instance ) => callback( this, instance );
		}
	}

	public class ThreadLocalStore<T> : WritableStore<T>
	{
		readonly ConditionMonitor monitor = new ConditionMonitor();

		readonly ThreadLocal<T> local;

		// int threads;

		public ThreadLocalStore( [Required]Func<T> create ) : this( new ThreadLocal<T>( create ) ) {}

		public ThreadLocalStore( ThreadLocal<T> local )
		{
			this.local = local;
		}

		public override void Assign( T item )
		{
			/*var update = !item.IsNull() ? threads | Environment.CurrentManagedThreadId : threads & ~Environment.CurrentManagedThreadId;
			Interlocked.Exchange( ref threads, update );*/
			local.Value = item;
		}

		protected override T Get()
		{
			/*if ( !local.IsValueCreated )
			{
				Interlocked.Exchange( ref threads, threads | Environment.CurrentManagedThreadId );
			}*/
			return local.Value;
		}

		// public bool IsDisposed => monitor.IsApplied;

		protected override void OnDispose()
		{
			/*Interlocked.Exchange( ref threads, threads & ~Environment.CurrentManagedThreadId );
			if ( monitor.ApplyIf( threads == 0 ) )
			{
				local.Dispose();
			}*/
			local.Dispose();
			
			base.OnDispose();
		}
	}

	public class EqualityReference<T> : AttachedPropertyBase<object, T>
	{
		readonly AttachedProperty<object, ConcurrentDictionary<int, T>> property = new AttachedProperty<ConcurrentDictionary<int, T>>( ActivatedAttachedPropertyStore<ConcurrentDictionary<int, T>>.Instance );

		public override bool IsAttached( object instance ) => property.Get( Execution.Current ).ContainsKey( instance.GetHashCode() );

		public override void Set( object instance, T value ) => property.Get( Execution.Current )[instance.GetHashCode()] = value;

		public override T Get( object instance ) => property.Get( Execution.Current ).GetOrAdd( instance.GetHashCode(), i => (T)instance );

		public override bool Clear( object instance )
		{
			property.Get( Execution.Current ).Clear();
			return true;
		}
	}
	
	public class Condition : AttachedProperty<ConditionMonitor>
	{
		public static Condition Property { get; } = new Condition();
		public Condition() : base( key => new ConditionMonitor() ) {}
	}
}