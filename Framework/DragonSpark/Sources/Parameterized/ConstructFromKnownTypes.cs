using DragonSpark.Sources.Scopes;
using DragonSpark.TypeSystem;

namespace DragonSpark.Sources.Parameterized
{
	public sealed class ConstructFromKnownTypes<T> : ParameterizedSingletonScope<object, T>
	{
		public static ConstructFromKnownTypes<T> Default { get; } = new ConstructFromKnownTypes<T>();
		ConstructFromKnownTypes() : base( o => new DefaultImplementation().ToSingleton() ) {}

		sealed class DefaultImplementation : FirstParameterConstructedSelector<T>
		{
			public DefaultImplementation() : base( KnownTypes<T>.Default.Unwrap() ) {}
		}
	}
}