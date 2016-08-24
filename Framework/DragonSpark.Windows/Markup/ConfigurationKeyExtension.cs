using System.Windows.Markup;

namespace DragonSpark.Windows.Markup
{
	[MarkupExtensionReturnType( typeof(string) )]
	public abstract class ConfigurationKeyExtension : MarkupExtensionBase
	{
		readonly string key;

		protected ConfigurationKeyExtension( string key )
		{
			this.key = key;
		}

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => key;
	}
}