using DragonSpark.Activation;
using DragonSpark.Extensions;
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

	public abstract class AttachedSetProperty<TInstance, TItem> : AttachedProperty<TInstance, ISet<TItem>> where TInstance : class
	{
		protected AttachedSetProperty() : base( key => new HashSet<TItem>() ) {}
		protected AttachedSetProperty( Func<TInstance, ISet<TItem>> create ) : base( create ) {}
	}

	public abstract class AttachedCollectionProperty<TItem> : AttachedCollectionProperty<object, TItem>, IAttachedProperty<ICollection<TItem>>
	{
		protected AttachedCollectionProperty() {}
		protected AttachedCollectionProperty( Func<object, ICollection<TItem>> create ) : base( create ) {}
	}
	
	public abstract class AttachedCollectionProperty<TInstance, TItem> : AttachedProperty<TInstance, ICollection<TItem>> where TInstance : class
	{
		protected AttachedCollectionProperty() : base( key => new System.Collections.ObjectModel.Collection<TItem>() ) {}
		protected AttachedCollectionProperty( Func<TInstance, ICollection<TItem>> create ) : base( create ) {}
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

	// [Synchronized]
	public class AttachedProperty<TInstance, TValue> : IAttachedProperty<TInstance, TValue> where TInstance : class
	{
		readonly ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback create;
		
		readonly ConditionalWeakTable<TInstance, IWritableStore<TValue>> items = new ConditionalWeakTable<TInstance, IWritableStore<TValue>>();

		public AttachedProperty() : this( instance => default(TValue) ) {}

		public AttachedProperty( Func<TInstance, TValue> create ) : this( new AssignedAttachedPropertyStore<TInstance, TValue>( create ) ) {}

		public AttachedProperty( AttachedPropertyStore<TInstance, TValue> create ) : this( new ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback( create.Create ) ) {}

		AttachedProperty( ConditionalWeakTable<TInstance, IWritableStore<TValue>>.CreateValueCallback create )
		{
			this.create = create;
		}

		public bool IsAttached( TInstance instance )
		{
			IWritableStore<TValue> temp;
			return items.TryGetValue( instance, out temp );
		}

		public virtual void Set( TInstance instance, TValue value ) => GetValue( instance ).Assign( value );

		public virtual TValue Get( TInstance instance ) => GetValue( instance ).Value;

		IWritableStore<TValue> GetValue( TInstance instance ) => items.GetValue( instance, create );
	}

	public class ActivatedAttachedPropertyStore<TValue> : ActivatedAttachedPropertyStore<object, TValue> where TValue : new()
	{
		public new static ActivatedAttachedPropertyStore<TValue> Instance { get; } = new ActivatedAttachedPropertyStore<TValue>();
	}

	public class ActivatedAttachedPropertyStore<TInstance, TValue> : AssignedAttachedPropertyStore<TInstance, TValue> where TValue : new() where TInstance : class
	{
		public new static ActivatedAttachedPropertyStore<TInstance, TValue> Instance { get; } = new ActivatedAttachedPropertyStore<TInstance, TValue>();

		public ActivatedAttachedPropertyStore() : base( instance => new TValue() ) {}
	}

	public abstract class ProjectedStore<TInstance, TValue> : AssignedAttachedPropertyStore<object, TValue>
	{
		protected override TValue CreateValue( object instance ) => instance.AsTo<TInstance, TValue>( Project );

		protected abstract TValue Project( TInstance instance );
	}



	public class AssignedAttachedPropertyStore<TInstance, TValue> : AttachedPropertyStore<TInstance, TValue> where TInstance : class
	{
		public new static AssignedAttachedPropertyStore<TInstance, TValue> Instance { get; } = new AssignedAttachedPropertyStore<TInstance, TValue>();

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

	public class AttachedPropertyStore<TInstance, TValue> where TInstance : class
	{
		public static AttachedPropertyStore<TInstance, TValue> Instance { get; } = new AttachedPropertyStore<TInstance, TValue>();

		readonly Func<TInstance, IWritableStore<TValue>> store;

		protected AttachedPropertyStore() : this( instance => new FixedStore<TValue>() ) {}

		public AttachedPropertyStore( Func<TInstance, IWritableStore<TValue>> store )
		{
			this.store = store;
		}

		public virtual IWritableStore<TValue> Create( TInstance instance ) => store( instance );
	}
}