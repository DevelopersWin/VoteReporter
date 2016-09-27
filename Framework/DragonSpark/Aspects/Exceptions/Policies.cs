namespace DragonSpark.Aspects.Exceptions
{
	public static class Policies
	{
		// readonly static IParameterizedSource<Policy, ICommand<Action>> Commands = new DecoratedCache<Policy, ApplyPolicyCommand>();

		/*public static void Retry<T>( Action action ) where T : Exception => Apply( Defaults<T>.Retry.ToDelegate(), action );

		static void Apply( Func<Policy> policy, Action action ) => Commands.Get( policy() ).Execute( action );*/

		/*public static ICommand<T> Apply<T>( this ICommand<T> @this, ISource<Policy> source ) => Apply( @this, source.ToDelegate() );
		public static ICommand<T> Apply<T>( this ICommand<T> @this, Func<Policy> source ) => new DeferredPolicyCommand<T>( source, @this );*/
	}

	/*public static class Defaults<T> where T : Exception
	{
		public static ISource<Policy> Retry { get; } = new Scope<Policy>( Exceptions.Retry.Create<T>().GlobalCache() );
	}*/
}
