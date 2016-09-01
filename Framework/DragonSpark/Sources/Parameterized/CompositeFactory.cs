using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

namespace DragonSpark.Sources.Parameterized
{
	public class CompositeFactory<TParameter, TResult> : ValidatedParameterizedSourceBase<TParameter, TResult>
	{
		readonly ImmutableArray<IParameterizedSource<TParameter, TResult>> sources;
		
		public CompositeFactory( params IParameterizedSource<TParameter, TResult>[] sources ) : base( new AnySpecification<TParameter>( sources.Select( source => source.ToSpecification<TParameter>() ).ToArray() ) )
		{
			this.sources = sources.ToImmutableArray();
		}

		public override TResult Get( [Optional]TParameter parameter ) => sources.Introduce( parameter, tuple => tuple.Item1.Get( tuple.Item2 ) ).FirstAssigned();

		protected override object GetGeneralized( object parameter ) => sources.Introduce( parameter, tuple => tuple.Item1.Get( tuple.Item2 ) ).FirstAssigned();
	}
}