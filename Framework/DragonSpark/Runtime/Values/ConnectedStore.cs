using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using Nito.ConnectedProperties;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DragonSpark.Runtime.Values
{
	public static class Ambient
	{
		public static object GetCurrent( [Required]Type type ) => typeof(Ambient).InvokeGeneric( nameof(GetCurrent), type.ToItem() );

		public static T GetCurrent<T>() => new ThreadAmbientChain<T>().Value.PeekOrDefault();

		public static T[] GetCurrentChain<T>() => new ThreadAmbientChain<T>().Value.ToArray();
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

	public class DisposableRepository : RepositoryBase<IDisposable>
	{
		public static DisposableRepository Instance { get; } = new DisposableRepository();

		DisposableRepository() {}

		public void DisposeAll()
		{
			var disposables = List();
			disposables.Each( disposable => disposable.Dispose() );
			Store.Clear();
		}
	}

	public abstract class ConnectedStore<T> : WritableStore<T>
	{
		/*readonly static ConcurrentDictionary<Tuple<object, string>, ConnectibleProperty<T>> Cache = new ConcurrentDictionary<Tuple<object, string>, ConnectibleProperty<T>>();

		static ConnectedStore()
		{
			DisposableRepository.Instance.Add( new DisposableAction( () =>
																	 {
																		 Cache.Values.Each( property => property.TryDisconnect() );
																		 Cache.Clear();
																		 Debugger.Break();
																	 } ) );
		}*/

		readonly Func<T> create;

		protected ConnectedStore( [Required] object source, Type type, Func<T> create = null ) : this( source, type.AssemblyQualifiedName, create ) {}

		protected ConnectedStore( [Required] object source, [NotEmpty] string name, Func<T> create = null ) : this( 
			// Cache.GetOrAdd( new Tuple<object, string>( instance, name ), t => PropertyConnector.Default.Get( t.Item1, t.Item2, true ).Cast<T>() )
			PropertyConnector.Default.Get( source, name, true ).Cast<T>()
			
			, create ) {}

		protected ConnectedStore( [Required]ConnectibleProperty<T> property, Func<T> create )
		{
			Property = property;
			this.create = create ?? DefaultValueFactory<T>.Instance.Create;
		}

		public override void Assign( T item ) => Property.Set( item );

		protected override T Get() => Property.GetOrCreate( create );

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

	public class Reference<T> : ConnectedStore<T>
	{
		public Reference( object source, T key ) : base( source, KeyFactory.Instance.CreateUsing( key ).ToString(), () => key ) {}
	}

	/*public class ListStore<T> : FixedStore<T>
	{
		readonly IList list;

		public ListStore( [Required] IList list )
		{
			this.list = list;
		}

		protected override void OnAssign( T item )
		{
			if ( item == null )
			{
				Remove( Value );
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

		protected override void OnDispose() => Remove( Value );
	}*/

	public class Checked : AssociatedStore<ConditionMonitor>
	{
		public Checked( object source ) : this( source, source ) {}

		public Checked( object source, [Required]object reference ) : this( source, KeyFactory.Instance.CreateUsing( reference ).ToString() ) {}

		public Checked( [Required]object source, [Required]string key ) : base( source, key, () => new ConditionMonitor() ) { }

		protected Checked( [Required]object source, [Required]Type key ) : base( source, key, () => new ConditionMonitor() ) { }
	}

	public class ThreadAmbientStore<T> : AssociatedStore<T>
	{
		public ThreadAmbientStore( Func<T> create = null ) : base( ThreadAmbientContext.GetCurrent(), typeof(T), create ) {}

		public ThreadAmbientStore( string key, Func<T> create = null ) : base( ThreadAmbientContext.GetCurrent(), key, create ) {}

		public ThreadAmbientStore( Type key, Func<T> create = null ) : base( ThreadAmbientContext.GetCurrent(), key, create ) {}
	}

	public class AssociatedStore<T> : AssociatedStore<object, T>
	{
		public AssociatedStore( object source, Func<T> create = null ) : this( source, typeof(AssociatedStore<object, T>), create ) {}

		public AssociatedStore( object source, string key, Func<T> create = null ) : base( source, key, create ) {}

		protected AssociatedStore( object source, Type key, Func<T> create = null ) : base( source, key, create ) {}
	}

	public class AssociatedStore<T, U> : ConnectedStore<U>
	{
		public AssociatedStore( T source, Func<U> create = null ) : this( source, typeof(AssociatedStore<T, U>), create ) {}

		protected AssociatedStore( T source, string key, Func<U> create = null ) : base( source, key, create ) {}

		protected AssociatedStore( T source, Type key, Func<U> create = null ) : base( source, key, create ) {}
	}

	public class Items : Items<object>
	{
		public Items( object source ) : base( source ) {}

		// public Items( object instance, Type key ) : base( instance, key ) {}
	}

	public class Items<T> : ConnectedStore<IList<T>>
	{
		public Items( object source ) : this( source, typeof(Items<T>) ) {}

		public Items( object source, Type key ) : base( source, key, () => new List<T>() ) {}

		// public TItem Get<TItem>() => Item.FirstOrDefaultOfType<TItem>();
	}

	/*class Tracked<T> : AssociatedValue<T>
	{
		public Tracked( object instance, ConnectedValue<T> inner ) : base( instance, typeof(Tracked<T>), () =>
		{
			new TrackedActions().Item.Add( () => inner.Property.TryDisconnect() );
			return inner.Item;
		} ) {}
	}*/

	/*class TrackedActions : Items<Action>
	{
		public TrackedActions() : this( Execution.Current ) {}

		public TrackedActions( object instance ) : base( instance, typeof(TrackedActions) ) {}

		public void Clear()
		{
			var actions = Item.Purge();
			actions.Each( action => action() );
		}
	}*/
}