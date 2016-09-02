using DragonSpark.Extensions;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace DragonSpark.Sources.Parameterized
{
	public class CompositeFactory<TParameter, TResult> : DecoratedParameterizedSource<TParameter, TResult>
	{
		public CompositeFactory( params IParameterizedSource<TParameter, TResult>[] sources ) : this( sources.ToImmutableArray() ) {}
		CompositeFactory( ImmutableArray<IParameterizedSource<TParameter, TResult>> sources ) : base( new Inner( sources ) ) {}

		sealed class Inner : ParameterizedSourceBase<TParameter, TResult>
		{
			readonly ImmutableArray<IParameterizedSource<TParameter, TResult>> sources;

			public Inner( ImmutableArray<IParameterizedSource<TParameter, TResult>> sources )
			{
				this.sources = sources;
			}

			public override TResult Get( [Optional]TParameter parameter ) => sources.Introduce( parameter, tuple => tuple.Item1.Get( tuple.Item2 ) ).FirstAssigned();

			// protected override object GetGeneralized( object parameter ) => sources.Introduce( parameter, tuple => tuple.Item1.Get( tuple.Item2 ) ).FirstAssigned();
		}
	}
}