using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

namespace DragonSpark.Sources.Parameterized
{
	public class CompositeFactory<TParameter, TResult> : ValidatedParameterizedSourceBase<TParameter, TResult>
	{
		readonly ImmutableArray<Func<TParameter, TResult>> inner;

		public CompositeFactory( params IParameterizedSource<TParameter, TResult>[] factories ) : this( factories.Select( factory => factory.ToSourceDelegate() ).ToArray() ) {}

		public CompositeFactory( params Func<TParameter, TResult>[] inner ) : this( Specifications<TParameter>.Always, inner ) {}

		public CompositeFactory( ISpecification<TParameter> specification, params Func<TParameter, TResult>[] inner ) : this( Defaults<TParameter>.Coercer, specification, inner ) {}

		public CompositeFactory( Coerce<TParameter> coercer, ISpecification<TParameter> specification, params Func<TParameter, TResult>[] inner ) : base( coercer, specification )
		{
			this.inner = inner.ToImmutableArray();
		}

		public override TResult Get( [Optional]TParameter parameter ) => inner.Introduce( parameter ).FirstAssigned();
	}
}