using PostSharp.Patterns.Contracts;

namespace DragonSpark.Windows.Markup
{
	public abstract class MarkupPropertyBase : IMarkupProperty
	{
		protected MarkupPropertyBase( [Required]PropertyReference reference )
		{
			Reference = reference;
		}

		public PropertyReference Reference { get; }

		public object GetValue() => OnGetValue();

		protected abstract object OnGetValue();

		public object SetValue( object value ) => Apply( value );

		protected abstract object Apply( object value );
	}
}