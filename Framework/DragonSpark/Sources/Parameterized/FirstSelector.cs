using DragonSpark.Specifications;
using System;
using System.Linq;

namespace DragonSpark.Sources.Parameterized
{
	public class FirstSelector<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TResult, bool> specification;
		readonly IParameterizedItemSource<TParameter, TResult> sources;

		public FirstSelector( params IParameterizedSource<TParameter, TResult>[] sources ) : this( Common<TResult>.Assigned, sources ) {}

		public FirstSelector( ISpecification<TResult> specification, params IParameterizedSource<TParameter, TResult>[] sources ) : 
			this( specification.ToDelegate(), new CompositeFactory<TParameter, TResult>( sources ) ) {}

		public FirstSelector( Func<TResult, bool> specification, IParameterizedItemSource<TParameter, TResult> sources )
		{
			this.specification = specification;
			this.sources = sources;
		}

		public override TResult Get( TParameter parameter ) => sources.Yield( parameter ).FirstOrDefault( specification );
	}
}