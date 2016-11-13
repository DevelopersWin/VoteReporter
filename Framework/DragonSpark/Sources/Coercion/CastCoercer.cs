namespace DragonSpark.Sources.Coercion
{
	public sealed class CastCoercer<T> : DelegatedCoercer<object, T>
	{
		public static CastCoercer<T> Default { get; } = new CastCoercer<T>();
		CastCoercer() : base( CastCoercer<object, T>.Default.Get ) {}
	}
}