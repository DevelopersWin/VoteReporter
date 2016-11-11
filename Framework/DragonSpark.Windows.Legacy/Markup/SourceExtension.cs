using DragonSpark.Sources;
using PostSharp.Patterns.Contracts;
using System.Windows.Markup;

namespace DragonSpark.Windows.Legacy.Markup
{
	[ContentProperty( nameof(Instance) )]
	public class SourceExtension : MarkupExtensionBase
	{
		public SourceExtension() {}

		public SourceExtension( ISourceAware instance )
		{
			Instance = instance;
		}

		[NotNull]
		public ISourceAware Instance { [return: NotNull]get; set; }

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => Instance.Get();
	}
}