using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PostSharp.Patterns.Contracts;
using Type = System.Type;

namespace DragonSpark.Windows.Markup
{
	public class CollectionMarkupProperty : MarkupPropertyBase
	{
		readonly IList collection;
		
		public CollectionMarkupProperty( [Required]IList collection, [Required]PropertyReference reference ) : base( reference )
		{
			this.collection = collection;
		}

		protected override object OnGetValue() => collection;

		protected override object Apply( object value )
		{
			var index = collection.Cast<object>().WithFirst( o => o == null, o =>
			{
				var i = collection.IndexOf( o );
				collection.RemoveAt( i );
				return i;
			}, () => -1 );

			var itemType = collection.GetType().Adapt().GetInnerType();
			var items = new Stack<object>( itemType.IsInstanceOfType( value ) ? value.ToItem() : value.GetType().Adapt().GetInnerType().With( itemType.IsAssignableFrom ) ? value.To<IEnumerable>().Cast<object>().Reverse() : Default<object>.Items );

			var result = index == -1 && items.Any() ? items.Pop() : null;

			var insert = Math.Max( 0, index );

			items.Each( o => collection.Insert( insert, o ) );

			return result;
		}
	}
}