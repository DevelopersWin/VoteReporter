using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DragonSpark.Runtime.Values
{
	public static class AttachedPropertyExtensions
	{
		public static TValue Get<TInstance, TValue>( this TInstance @this, IAttachedProperty<TInstance, TValue> property ) where TInstance : class => property.Get( @this );

		public static void Set<TInstance, TValue>( this TInstance @this, IAttachedProperty<TInstance, TValue> property, TValue value ) where TInstance : class => property.Set( @this, value );
	}

	public interface IAttachedProperty<TValue> : IAttachedProperty<object, TValue>
	{}

	public interface IAttachedProperty<in TInstance, TValue> where TInstance : class
	{
		bool IsAttached( TInstance instance );

		void Set( TInstance instance, TValue value );

		TValue Get( TInstance instance );
	}

	/*class StoreConverter<T> : Converter<T, T>
	{
		public StoreConverter( IWritableStore<T> store ) : base( arg => store., @from ) {}
	}*/

	class SelfConverter<T> : Converter<T, T>
	{
		public static SelfConverter<T> Instance { get; } = new SelfConverter<T>();

		SelfConverter() : base( Default<T>.Self, Default<T>.Self ) {}
	}

	class TupleConverter<T> : Converter<T, Tuple<T>>
	{
		public static TupleConverter<T> Instance { get; } = new TupleConverter<T>();

		TupleConverter() : base( arg => new Tuple<T>( arg ), tuple => tuple.Item1 ) {}
	}

	public abstract class AttachedSetProperty<TItem> : AttachedSetProperty<object, TItem>, IAttachedProperty<ISet<TItem>>
	{
		protected AttachedSetProperty() {}
		protected AttachedSetProperty( Func<object, ISet<TItem>> create ) : base( create ) {}
	}

	public abstract class AttachedSetProperty<TInstance, TItem> : AttachedPropertyBase<TInstance, ISet<TItem>> where TInstance : class
	{
		protected AttachedSetProperty() : base( key => new HashSet<TItem>() ) {}
		protected AttachedSetProperty( Func<TInstance, ISet<TItem>> create ) : base( create ) {}
	}

	public abstract class AttachedCollectionProperty<TItem> : AttachedCollectionProperty<object, TItem>, IAttachedProperty<ICollection<TItem>>
	{
		protected AttachedCollectionProperty() {}
		protected AttachedCollectionProperty( Func<object, ICollection<TItem>> create ) : base( create ) {}
	}
	
	public abstract class AttachedCollectionProperty<TInstance, TItem> : AttachedPropertyBase<TInstance, ICollection<TItem>> where TInstance : class
	{
		protected AttachedCollectionProperty() : base( key => new System.Collections.ObjectModel.Collection<TItem>() ) {}
		protected AttachedCollectionProperty( Func<TInstance, ICollection<TItem>> create ) : base( create ) {}
	}

	public abstract class AttachedPropertyBase<TValue> : AttachedPropertyBase<object, TValue>, IAttachedProperty<TValue>
	{
		protected AttachedPropertyBase() {}
		protected AttachedPropertyBase( Func<object, TValue> create ) : base( create ) {}

		protected AttachedPropertyBase( Func<object, IWritableStore<TValue>> create ) : base( create ) {}
	}


	/*public abstract class AttachedPropertyBase<TInstance, TValue> : AttachedPropertyBase<TInstance, TValue, TValue> where TInstance : class where TValue : class
	{
		protected AttachedPropertyBase() : this( key => default(TValue) ) {}
		protected AttachedPropertyBase( ConditionalWeakTable<TInstance, TValue>.CreateValueCallback create ) : base( create, SelfConverter<TValue>.Instance ) {}
	}*/

	// [Synchronized]
	public abstract class AttachedPropertyBase<TInstance, TValue> : IAttachedProperty<TInstance, TValue> where TInstance : class
	{
		readonly ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback create;
		
		readonly ConditionalWeakTable<TInstance, IWritableStore<TValue>> items = new ConditionalWeakTable<TInstance, IWritableStore<TValue>>();

		protected AttachedPropertyBase() : this( instance => default(TValue) ) {}

		protected AttachedPropertyBase( Func<TInstance, TValue> create ) : this( new Func<TInstance, IWritableStore<TValue>>( instance => new FixedStore<TValue>().Assigned( create( instance ) ) ) ) {}

		protected AttachedPropertyBase( Func<TInstance, IWritableStore<TValue>> create ) : this( new ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback( create  ) ) {}

		AttachedPropertyBase( ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback create )
		{
			this.create = create;
		}

		public bool IsAttached( TInstance instance )
		{
			IWritableStore<TValue> temp;
			return items.TryGetValue( instance, out temp );
		}

		public void Set( TInstance instance, TValue value ) => GetValue( instance ).Assign( value );

		public TValue Get( TInstance instance ) => GetValue( instance ).Value;

		IWritableStore<TValue> GetValue( TInstance instance ) => items.GetValue( instance, create );
	}

	/*public abstract class AttachedPropertyBase<T, TValue> : IAttachedProperty<T, TValue> where TValue : class where T : class
	{
		readonly ConditionalWeakTable<T, TValue>.CreateValueCallback create;

		// [Child]
		// readonly ConcurrentDictionary<T, TValue> items = new ConcurrentDictionary<T, TValue>();
		readonly ConditionalWeakTable<T, TValue> items = new ConditionalWeakTable<T, TValue>();

		protected AttachedPropertyBase() : this( key => default(TValue) ) {}

		protected AttachedPropertyBase( ConditionalWeakTable<T, TValue>.CreateValueCallback create )
		{
			this.create = create;
		}

		// [Reader]
		public bool IsAttached( T instance )
		{
			TValue temp;
			return items.TryGetValue( instance, out temp );
			// return items.ContainsKey( instance );
		}

		// [Writer]
		public void Set( T instance, TValue value )
		{
			if ( IsAttached( instance ) )
			{
				items.Remove( instance );
			}
			items.Add( instance, value );
			// items.AddOrUpdate( instance, arg => value, ( arg1, value1 ) => value );
			// items[instance] = value;
		}

		// [Writer]
		public TValue Get( T instance ) => items.GetValue( instance, create );
	}*/
}
