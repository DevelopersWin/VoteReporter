using DragonSpark.Sources.Coercion;

namespace DragonSpark.Activation
{
	public sealed class ConstructCoercer<T> : DelegatedCoercer<object, T>
	{
		public static ConstructCoercer<T> Default { get; } = new ConstructCoercer<T>();
		ConstructCoercer() : base( ParameterConstructor<T>.From ) {}
	}
}