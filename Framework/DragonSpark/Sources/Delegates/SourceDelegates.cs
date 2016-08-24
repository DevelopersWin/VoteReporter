using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem.Generics;
using System;

namespace DragonSpark.Sources.Delegates
{
	public class SourceDelegates : DelegatesBase
	{
		public static IParameterizedSource<Func<IServiceProvider>, IParameterizedSource<Type, Delegate>> Sources { get; } = new Cache<Func<IServiceProvider>, SourceDelegates>( func => new SourceDelegates( func ) );
		SourceDelegates( Func<IServiceProvider> source ) : base( source.Delegate<object>(), IsSourceSpecification.Default, nameof(ToDelegate) ) {}

		protected override Delegate Create( Type parameter ) => Methods.Make( ResultTypes.Default.Get( parameter ) ).Invoke<Delegate>( Locator( parameter ) );

		static Delegate ToDelegate<T>( ISource<T> source ) => source.ToDelegate();
	}
}