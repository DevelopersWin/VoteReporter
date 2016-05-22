using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Collections;

namespace DragonSpark.Runtime.Values
{
	public static class AttachedPropertyExtensions
	{
		public static TValue Get<T, TValue>( this T @this, AttachedProperty<T, TValue> property ) where T : class where TValue : class => property.Get( @this );

		public static void Set<T, TValue>( this T @this, AttachedProperty<T, TValue> property, TValue value ) where T : class where TValue : class => property.Set( @this, value );
	}

	public abstract class AttachedValue<T, TValue> : AttachedProperty<T, Tuple<TValue>> where TValue : struct where T : class
	{
		protected AttachedValue() : this( arg => default(TValue) ) {}
		protected AttachedValue( Func<T, TValue> creator ) : this( arg => new Tuple<TValue>( creator( arg ) ) ) {}
		AttachedValue( ConditionalWeakTable<T, Tuple<TValue>>.CreateValueCallback create ) : base( create ) {}
	}

	// [ReaderWriterSynchronized]
	public abstract class AttachedProperty<T, TValue> where TValue : class where T : class
	{
		readonly ConditionalWeakTable<T, TValue>.CreateValueCallback create;

		// [Child]
		// readonly ConcurrentDictionary<T, TValue> items = new ConcurrentDictionary<T, TValue>();
		readonly ConditionalWeakTable<T, TValue> items = new ConditionalWeakTable<T, TValue>();

		protected AttachedProperty() : this( key => default(TValue) ) {}

		protected AttachedProperty( ConditionalWeakTable<T, TValue>.CreateValueCallback create )
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
	}
}
