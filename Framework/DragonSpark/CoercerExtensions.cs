using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark
{
	public static class CoercerExtensions
	{
		public static Coerce<T> ToDelegate<T>( this ICoercer<T> @this ) => DelegateCache<T>.Default.Get( @this );
		class DelegateCache<T> : Cache<ICoercer<T>, Coerce<T>>
		{
			public static DelegateCache<T> Default { get; } = new DelegateCache<T>();
			DelegateCache() : base( command => command.Coerce ) {}
		}
	}
}