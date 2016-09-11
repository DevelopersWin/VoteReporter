using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Aspects.Extensibility
{
	public static class Extensions
	{
		public static IInvocation<TParameter, TResult> Get<TParameter, TResult>( this IRootInvocation @this ) => @this.Get().Wrap<TParameter, TResult>();
		public static IInvocation<TParameter, TResult> Wrap<TParameter, TResult>( this IInvocation @this ) => @this as IInvocation<TParameter, TResult> ?? Wrappers<TParameter, TResult>.Default.Get( @this );
		sealed class Wrappers<TParameter, TResult> : Cache<IInvocation, IInvocation<TParameter, TResult>>
		{
			public static Wrappers<TParameter, TResult> Default { get; } = new Wrappers<TParameter, TResult>();
			Wrappers() : base( result => new WrappedInvocation<TParameter, TResult>( result ) ) {}
		}
	}
}