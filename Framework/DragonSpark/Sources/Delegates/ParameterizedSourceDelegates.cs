using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem.Generics;
using System;

namespace DragonSpark.Sources.Delegates
{
	public class ParameterizedSourceDelegates : DelegatesBase
	{
		public static IParameterizedSource<Func<IServiceProvider>, IParameterizedSource<Type, Delegate>> Sources { get; } = new Cache<Func<IServiceProvider>, ParameterizedSourceDelegates>( func => new ParameterizedSourceDelegates( func ) );
		ParameterizedSourceDelegates( Func<IServiceProvider> source ) : base( source.Delegate<object>(), IsParameterizedSourceSpecification.Default, nameof(ToDelegate) ) {}

		protected override Delegate Create( Type parameter ) => 
			Methods
				.Make( ParameterTypes.Default.Get( parameter ), ResultTypes.Default.Get( parameter ) )
				.Invoke<Delegate>( Locator( parameter ) );

		static Delegate ToDelegate<TParameter, TResult>( IParameterizedSource<TParameter, TResult> source ) => source.ToSourceDelegate();
	}
}