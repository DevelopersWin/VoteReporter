namespace DragonSpark
{
	public sealed class StringCoercer : Coercer<string>
	{
		public new static StringCoercer Default { get; } = new StringCoercer();
		StringCoercer() {}

		protected override string PerformCoercion( object parameter = null ) => parameter?.ToString();
	}
}