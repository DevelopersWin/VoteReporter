using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Windows.Markup;

namespace DragonSpark.Windows.Markup
{
	/*public class EnumerableType : MarkupExtension
	{
		readonly Type elementType;

		public EnumerableType( Type elementType )
		{
			this.elementType = elementType;
		}

		public override object ProvideValue( IServiceProvider serviceProvider )
		{
			var result = typeof(IEnumerable<>).MakeGenericType( elementType );
			return result;
		}
	}*/

	[MarkupExtensionReturnType( typeof(Type) )]
	public class GenericTypeExtension : MarkupExtensionBase
	{
		public GenericTypeExtension( string typeName )
		{
			TypeName = typeName;
		}

		[NotEmpty]
		public string TypeName { [return: NotEmpty]get; set; }

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => serviceProvider.Get<IXamlTypeResolver>().Resolve( TypeName );
	}
}
