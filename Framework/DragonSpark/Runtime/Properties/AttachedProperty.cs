using DragonSpark.Runtime.Stores;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DragonSpark.Runtime.Properties
{
	public static class AttachedPropertyExtensions
	{
		public static TValue Get<TInstance, TValue>( this TInstance @this, IAttachedProperty<TInstance, TValue> property ) where TInstance : class => property.Get( @this );

		public static void Set<TInstance, TValue>( this TInstance @this, IAttachedProperty<TInstance, TValue> property, TValue value ) where TInstance : class => property.Set( @this, value );

		public static Assignment<PropertyAssign<T1, T2>, T1, T2> Assignment<T1, T2>( this IAttachedProperty<T1, T2> @this, T1 first, T2 second ) where T1 : class => 
			new Assignment<PropertyAssign<T1, T2>, T1, T2>( new PropertyAssign<T1, T2>( @this ), Assignments.From( first ), new Value<T2>( second ) );
	}

	public interface IAttachedProperty<TValue> : IAttachedProperty<object, TValue> {}

	public interface IAttachedProperty<in TInstance, TValue> where TInstance : class
	{
		bool IsAttached( TInstance instance );

		void Set( TInstance instance, TValue value );

		TValue Get( TInstance instance );

		bool Clear( TInstance instance );

		// void Dispose( TInstance instance );
	}

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

	public class ThreadLocalAttachedProperty<T> : ThreadLocalAttachedProperty<object, T>, IAttachedProperty<T>
	{
		public ThreadLocalAttachedProperty() : this( () => default(T) ) {}
		public ThreadLocalAttachedProperty( Func<T> create ) : base( create ) {}

		protected ThreadLocalAttachedProperty( IAttachedPropertyStore<object, T> store ) : base( store ) {}

		// protected ThreadLocalAttachedProperty( Func<object, IWritableStore<T>> store ) : base( store ) {}
	}

	public class ThreadLocalAttachedProperty<TInstance, TResult> : AttachedProperty<TInstance, TResult> where TInstance : class
	{
		public ThreadLocalAttachedProperty( Func<TResult> create ) : this( new Store( create ) ) {}

		protected ThreadLocalAttachedProperty( IAttachedPropertyStore<TInstance, TResult> store ) : base( store ) {}

		/*protected override bool Remove( TInstance instance, IWritableStore<TResult> store )
		{
			var local = store as ThreadLocalStore<TResult>;
			return ( local == null || local.IsDisposed ) && base.Remove( instance, store );
		}*/

		class Store : AttachedPropertyStoreBase<TInstance, TResult>
		{
			readonly Func<TResult> create;

			public Store( Func<TResult> create )
			{
				this.create = create;
			}

			public override IWritableStore<TResult> Create( TInstance instance ) => new ThreadLocalStore<TResult>( create );
		}
	}

	public class AttachedSetProperty<T> : AttachedSetProperty<object, T>, IAttachedProperty<ISet<T>>
	{
		public AttachedSetProperty() {}
		public AttachedSetProperty( Func<object, ISet<T>> create ) : base( create ) {}
	}

	public class AttachedSetProperty<TInstance, TItem> : AttachedProperty<TInstance, ISet<TItem>> where TInstance : class
	{
		public AttachedSetProperty() : base( key => new HashSet<TItem>() ) {}
		public AttachedSetProperty( Func<TInstance, ISet<TItem>> create ) : base( create ) {}
	}

	public class AttachedCollectionProperty : AttachedCollectionProperty<object>
	{
		public new static AttachedCollectionProperty Default { get; } = new AttachedCollectionProperty();

		public AttachedCollectionProperty() {}
		public AttachedCollectionProperty( Func<object, ICollection<object>> create ) : base( create ) {}
	}

	public class AttachedCollectionProperty<TItem> : AttachedCollectionProperty<object, TItem>, IAttachedProperty<ICollection<TItem>>
	{
		public new static AttachedCollectionProperty Default { get; } = new AttachedCollectionProperty();

		public AttachedCollectionProperty() {}
		public AttachedCollectionProperty( Func<object, ICollection<TItem>> create ) : base( create ) {}
	}
	
	public class AttachedCollectionProperty<TInstance, TItem> : AttachedProperty<TInstance, ICollection<TItem>> where TInstance : class
	{
		public static AttachedCollectionProperty Default { get; } = new AttachedCollectionProperty();

		public AttachedCollectionProperty() : base( key => new System.Collections.ObjectModel.Collection<TItem>() ) {}
		public AttachedCollectionProperty( Func<TInstance, ICollection<TItem>> create ) : base( create ) {}
	}

	public class AttachedProperty<TValue> : AttachedProperty<object, TValue>, IAttachedProperty<TValue>
	{
		public AttachedProperty() {}
		public AttachedProperty( Func<object, TValue> create ) : this( create, o => new FixedStore<TValue>() ) {}

		public AttachedProperty( Func<object, IWritableStore<TValue>> store ) : this( new AttachedPropertyStore<object, TValue>( store ) ) {}
		public AttachedProperty( Func<object, TValue> create, Func<object, IWritableStore<TValue>> store ) : base( new AssignedAttachedPropertyStore<object, TValue>( create, store ) ) {}

		public AttachedProperty( AttachedPropertyStore<object, TValue> create ) : base( create ) {}
	}


	/*public abstract class AttachedPropertyBase<TInstance, TValue> : AttachedPropertyBase<TInstance, TValue, TValue> where TInstance : class where TValue : class
	{
		protected AttachedPropertyBase() : this( key => default(TValue) ) {}
		protected AttachedPropertyBase( ConditionalWeakTable<TInstance, TValue>.CreateValueCallback create ) : base( create, SelfConverter<TValue>.Instance ) {}
	}*/

	public abstract class AttachedPropertyBase<TInstance, TValue> : IAttachedProperty<TInstance, TValue> where TInstance : class
	{
		public abstract bool IsAttached( TInstance instance );
		public abstract void Set( TInstance instance, TValue value );
		public abstract TValue Get( TInstance instance );
		public abstract bool Clear( TInstance instance );
	}

	// [Synchronized]
	public class AttachedProperty<TInstance, TValue> : AttachedPropertyBase<TInstance, TValue> where TInstance : class
	{
		// readonly AttachedPropertyStore<TInstance, TValue> store;
		readonly ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback create;
		
		readonly ConditionalWeakTable<TInstance, IWritableStore<TValue>> items = new ConditionalWeakTable<TInstance, IWritableStore<TValue>>();

		public AttachedProperty() : this( instance => default(TValue) ) {}

		public AttachedProperty( Func<TInstance, TValue> create ) : this( new AssignedAttachedPropertyStore<TInstance, TValue>( create ) ) {}

		public AttachedProperty( Func<TInstance, IWritableStore<TValue>> store ) : this( new AttachedPropertyStore<TInstance, TValue>( store ) ) {}

		public AttachedProperty( IAttachedPropertyStore<TInstance, TValue> store ) : this( new ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback( store.Create ) ) {}

		AttachedProperty( ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback create )
		{
			this.create = create;
		}

		public override bool IsAttached( TInstance instance )
		{
			IWritableStore<TValue> temp;
			return items.TryGetValue( instance, out temp );
		}

		public override void Set( TInstance instance, TValue value ) => items.GetValue( instance, create ).Assign( value );

		public override TValue Get( TInstance instance ) => items.GetValue( instance, create ).Value;

		public override bool Clear( TInstance instance ) => items.Remove( instance );

		//protected virtual bool Remove( TInstance instance, IWritableStore<TValue> store ) => ;

		/*protected virtual IWritableStore<TValue> Dispose( TInstance instance )
		{
			IWritableStore<TValue> item;
			if ( items.TryGetValue( instance, out item ) )
			{
				item.TryDispose();
			}
			return item;
		}*/
	}

	public class ActivatedAttachedProperty<TResult> : ActivatedAttachedProperty<object, TResult>, IAttachedProperty<TResult> where TResult : new() {}

	public class ActivatedAttachedProperty<TInstance, TResult> : AttachedProperty<TInstance, TResult> where TInstance : class where TResult : new()
	{
		public ActivatedAttachedProperty() : base( ActivatedAttachedPropertyStore<TInstance, TResult>.Instance ) {}
	}
	

	public class ActivatedAttachedPropertyStore<TValue> : ActivatedAttachedPropertyStore<object, TValue> where TValue : new()
	{
		public new static ActivatedAttachedPropertyStore<TValue> Instance { get; } = new ActivatedAttachedPropertyStore<TValue>();
	}

	public class ActivatedAttachedPropertyStore<TInstance, TValue> : AssignedAttachedPropertyStore<TInstance, TValue> where TValue : new() where TInstance : class
	{
		public static ActivatedAttachedPropertyStore<TInstance, TValue> Instance { get; } = new ActivatedAttachedPropertyStore<TInstance, TValue>();

		public ActivatedAttachedPropertyStore() : base( instance => new TValue() ) {}
	}

	public abstract class ProjectedStore<TInstance, TValue> : AssignedAttachedPropertyStore<object, TValue>
	{
		protected override TValue CreateValue( object instance ) => instance is TInstance ? Project( (TInstance)instance ) : default(TValue);

		protected abstract TValue Project( TInstance instance );
	}



	public class AssignedAttachedPropertyStore<TInstance, TValue> : AttachedPropertyStore<TInstance, TValue> where TInstance : class
	{
		//public new static AssignedAttachedPropertyStore<TInstance, TValue> Instance { get; } = new AssignedAttachedPropertyStore<TInstance, TValue>();

		readonly Func<TInstance, TValue> create;

		protected AssignedAttachedPropertyStore() : this( instance => default(TValue) ) {}

		public AssignedAttachedPropertyStore( Func<TInstance, TValue> create ) : this( create, instance => new FixedStore<TValue>() ) {}

		public AssignedAttachedPropertyStore( Func<TInstance, TValue> create, Func<TInstance, IWritableStore<TValue>> store ) : base( store )
		{
			this.create = create;
		}

		protected virtual TValue CreateValue( TInstance instance ) => create( instance );

		public override IWritableStore<TValue> Create( TInstance instance ) => base.Create( instance ).Assigned( CreateValue( instance ) );
	}

	public interface IAttachedPropertyStore<in TInstance, TValue> where TInstance : class
	{
		IWritableStore<TValue> Create( TInstance instance );
	}

	public abstract class AttachedPropertyStoreBase<TInstance, TValue> : IAttachedPropertyStore<TInstance, TValue> where TInstance : class
	{
		public abstract IWritableStore<TValue> Create( TInstance instance );
	}

	public class AttachedPropertyStore<TInstance, TValue> : AttachedPropertyStoreBase<TInstance, TValue> where TInstance : class
	{
		readonly Func<TInstance, IWritableStore<TValue>> store;

		public AttachedPropertyStore( Func<TInstance, IWritableStore<TValue>> store )
		{
			this.store = store;
		}

		public override IWritableStore<TValue> Create( TInstance instance ) => store( instance );
	}
}