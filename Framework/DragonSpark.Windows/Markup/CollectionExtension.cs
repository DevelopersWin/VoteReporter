using DragonSpark.Extensions;
using DragonSpark.Runtime;
using System.Collections;
using System.Linq;
using System.Windows.Markup;

namespace DragonSpark.Windows.Markup
{
	[ContentProperty( nameof(Items) )]
	public class CollectionExtension : MarkupExtensionBase
	{
		public Collection Items { get; } = new Collection();

		protected virtual IList DetermineCollection( MarkupServiceProvider serviceProvider )
		{
			var target = serviceProvider.TargetObject;
			var result = serviceProvider.Property.GetValue() as IList ?? target as IList;
			return result;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider )
		{
			var result = DetermineCollection( serviceProvider ).With( o =>
			{
				var type = o.GetType().Adapt().GetEnumerableType();
				Items.Where( type.IsInstanceOfType ).Each( item =>
				{
					o.Add( item );
				} );
				Items.Clear();
			} );
			return result;
		}
	}
}