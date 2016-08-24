using System.Windows.Markup;
using DragonSpark.Sources;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Windows.Markup
{
	[ContentProperty( nameof(Instance) )]
	public class SourceExtension : MarkupExtensionBase
	{
		public SourceExtension() {}

		public SourceExtension( ISource instance )
		{
			Instance = instance;
		}

		[Required]
		public ISource Instance { [return: Required]get; set; }

		protected override object GetValue( MarkupServiceProvider serviceProvider ) => Instance.Get();
	}
}