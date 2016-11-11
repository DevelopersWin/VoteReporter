using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Linq;

namespace DragonSpark.Sources.Parameterized
{
	public class FirstSelector<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TParameter, Func<TResult, bool>> specificationSource;
		readonly IParameterizedItemSource<TParameter, TResult> sources;

		public FirstSelector( params IParameterizedSource<TParameter, TResult>[] sources ) : this( Common<TResult>.Assigned, sources ) {}

		public FirstSelector( ISpecification<TResult> specification, params IParameterizedSource<TParameter, TResult>[] sources ) 
			: this ( specification, sources.Select( source => source.ToDelegate() ).Fixed() ) {}

		public FirstSelector( params Func<TParameter, TResult>[] sources ) : this( Common<TResult>.Assigned, sources ) {}

		public FirstSelector( ISpecification<TResult> specification, params Func<TParameter, TResult>[] sources )
			: this( specification.ToDelegate().Wrap, sources ) {}

		public FirstSelector( Func<TParameter, Func<TResult, bool>> specificationSource, params Func<TParameter, TResult>[] sources )
			: this( specificationSource, new CompositeFactory<TParameter, TResult>( sources ) ) {}

		public FirstSelector( Func<TParameter, Func<TResult, bool>> specificationSource, IParameterizedItemSource<TParameter, TResult> sources )
		{
			this.specificationSource = specificationSource;
			this.sources = sources;
		}

		public override TResult Get( TParameter parameter ) => sources.Yield( parameter ).FirstOrDefault( specificationSource( parameter ) );
	}
}