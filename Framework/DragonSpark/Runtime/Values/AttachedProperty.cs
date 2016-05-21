using System.Runtime.CompilerServices;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;

namespace DragonSpark.Runtime.Values
{
	public static class AttachedPropertyExtensions
	{
		public static TValue Get<T, TValue>( this T @this, AttachedProperty<T, TValue> property ) where T : class where TValue : class => property.Get( @this );

		public static void Set<T, TValue>( this T @this, AttachedProperty<T, TValue> property, TValue value ) where T : class where TValue : class => property.Set( @this, value );
	}

	[ReaderWriterSynchronized]
	public abstract class AttachedProperty<T, TValue> where TValue : class where T : class
	{
		readonly ConditionalWeakTable<T, TValue>.CreateValueCallback create;

		[Reference]
		readonly ConditionalWeakTable<T, TValue> items = new ConditionalWeakTable<T, TValue>();

		protected AttachedProperty() : this( key => default(TValue) ) {}

		protected AttachedProperty( ConditionalWeakTable<T, TValue>.CreateValueCallback create )
		{
			this.create = create;
		}

		[Reader]
		public bool Has( T instance )
		{
			TValue temp;
			return items.TryGetValue( instance, out temp );
		}

		[Writer]
		public void Set( T instance, TValue value )
		{
			if ( Has( instance ) )
			{
				items.Remove( instance );
			}
			items.Add( instance, value );
		}

		[Reader]
		public TValue Get( T instance ) => items.GetValue( instance, create );
	}
}
