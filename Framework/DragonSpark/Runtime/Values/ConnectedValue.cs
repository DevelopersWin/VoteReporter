using DragonSpark.Aspects;
using DragonSpark.Extensions;
using Nito.ConnectedProperties;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace DragonSpark.Runtime.Values
{
	public static class Ambient
	{
		public static object GetCurrent( [Required]Type type ) => typeof(Ambient).InvokeGeneric( nameof(GetCurrent), type.ToItem() );

		public static T GetCurrent<T>() => new ThreadAmbientChain<T>().Item.PeekOrDefault();

		public static T[] GetCurrentChain<T>() => new ThreadAmbientChain<T>().Item.ToArray();
	}

	public class ThreadLocalValue<T> : WritableValue<T>
	{
		readonly ThreadLocal<T> local;

		public ThreadLocalValue( [Required]Func<T> create ) : this( new ThreadLocal<T>( create ) ) {}

		public ThreadLocalValue( ThreadLocal<T> local )
		{
			this.local = local;
		}

		public override void Assign( T item ) => local.Value = item;

		public override T Item => local.Value;

		protected override void OnDispose()
		{
			local.Dispose();
			base.OnDispose();
		}
	}

	public abstract class ConnectedValue<T> : WritableValue<T>
	{
		readonly Func<T> create;

		protected ConnectedValue( [Required] object instance, Type type, Func<T> create = null ) : this( instance, type.GetHashCode().ToString(), create )
		{}

		protected ConnectedValue( [Required]object instance, [NotEmpty]string name, Func<T> create = null ) : this( PropertyConnector.Default.Get( instance, name, true ).Cast<T>(), create )
		{}

		protected ConnectedValue( [Required]ConnectibleProperty<T> property, Func<T> create )
		{
			Property = property;
			this.create = create ?? ( () => default(T) );
		}

		public override void Assign( T item ) => Property.Set( item );

		public override T Item => Property.GetOrCreate( create );

		public ConnectibleProperty<T> Property { get; }

		protected override void OnDispose()
		{
			Property.TryDisconnect();
			base.OnDispose();
		}
	}

	/*public class ConnectedValueKeyFactory<T> : Factory<EqualityList, string>
	{
		public static ConnectedValueKeyFactory<T> Instance { get; } = new ConnectedValueKeyFactory<T>();

		protected override string CreateItem( EqualityList parameter ) => $"{typeof(T)}-{parameter.GetHashCode()}";
	}*/

	/*public class ConnectedValueKeyFactory : Factory<object, string>
	{
		public static ConnectedValueKeyFactory Instance { get; } = new ConnectedValueKeyFactory();

		protected override string CreateItem( object parameter ) => $"{parameter.GetType()}-{parameter.GetHashCode()}";
	}*/

	public class Reference<T> : ConnectedValue<T>
	{
		public Reference( object instance, T key ) : base( instance, KeyFactory.Instance.CreateUsing( key ).ToString(), () => key ) {}
	}

	public class ListValue<T> : FixedValue<T>
	{
		readonly IList list;

		public ListValue( [Required] IList list )
		{
			this.list = list;
		}

		protected override void OnAssign( T item )
		{
			if ( item == null )
			{
				Remove( Item );
			}
			else if ( !list.Contains( item ) )
			{
				list.Add( item );
			}
			
			base.OnAssign( item );
		}

		void Remove( T item )
		{
			if ( item != null && list.Contains( item ) )
			{
				list.Remove( item );
			}
		}

		protected override void OnDispose() => Remove( Item );
	}

	public class Checked : AssociatedValue<ConditionMonitor>
	{
		public Checked( object instance ) : this( instance, instance ) {}

		public Checked( object instance, [Required]object reference ) : this( instance, KeyFactory.Instance.CreateUsing( reference ).ToString() ) {}

		public Checked( [Required]object instance, [Required]string key ) : base( instance, key, () => new ConditionMonitor() ) { }

		protected Checked( [Required]object instance, [Required]Type key ) : base( instance, key, () => new ConditionMonitor() ) { }
	}

	public class ThreadAmbientValue<T> : AssociatedValue<T>
	{
		public ThreadAmbientValue( Func<T> create = null ) : base( ThreadAmbientContext.GetCurrent(), typeof(T), create ) {}

		public ThreadAmbientValue( string key, Func<T> create = null ) : base( ThreadAmbientContext.GetCurrent(), key, create ) {}

		public ThreadAmbientValue( Type key, Func<T> create = null ) : base( ThreadAmbientContext.GetCurrent(), key, create ) {}
	}

	public class AssociatedValue<T> : AssociatedValue<object, T>
	{
		public AssociatedValue( object instance, Func<T> create = null ) : this( instance, typeof(AssociatedValue<object, T>), create ) {}

		public AssociatedValue( object instance, string key, Func<T> create = null ) : base( instance, key, create ) {}

		protected AssociatedValue( object instance, Type key, Func<T> create = null ) : base( instance, key, create ) {}
	}

	public class AssociatedValue<T, U> : ConnectedValue<U>
	{
		public AssociatedValue( T instance, Func<U> create = null ) : this( instance, typeof(AssociatedValue<T, U>), create ) {}

		protected AssociatedValue( T instance, string key, Func<U> create = null ) : base( instance, key, create ) {}

		protected AssociatedValue( T instance, Type key, Func<U> create = null ) : base( instance, key, create ) {}
	}

	public class Items : Items<object>
	{
		public Items( object instance ) : base( instance ) {}
	}

	public class Items<T> : ConnectedValue<IList<T>>
	{
		public Items( object instance ) : base( instance, typeof(Items<T>), () => new List<T>() ) {}

		public TItem Get<TItem>() => Item.FirstOrDefaultOfType<TItem>();
	}
}